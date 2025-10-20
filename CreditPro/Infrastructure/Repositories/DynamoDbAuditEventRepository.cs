using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditPro.Infrastructure.Repositories;

public class DynamoDbAuditEventRepository : IAuditEventRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly ILogger<DynamoDbAuditEventRepository> _logger;
    private bool _tableInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public DynamoDbAuditEventRepository(
        IAmazonDynamoDB dynamoDb,
        IConfiguration configuration,
        ILogger<DynamoDbAuditEventRepository> logger)
    {
        _dynamoDb = dynamoDb;
        _tableName = configuration["AWS:TableName"] ?? "CreditProAuditEvents";
        _logger = logger;

        // NO inicializar aquí - hacerlo lazy en el primer uso
    }

    private async Task EnsureTableExistsAsync(CancellationToken cancellationToken = default)
    {
        if (_tableInitialized) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_tableInitialized) return;

            _logger.LogInformation("🔍 Verificando tabla DynamoDB: {TableName}", _tableName);

            // Describe con timeout corto
            using var ctsDescribe = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ctsDescribe.CancelAfter(TimeSpan.FromSeconds(4));

            try
            {
                await _dynamoDb.DescribeTableAsync(_tableName, ctsDescribe.Token);
                _tableInitialized = true;
                _logger.LogInformation("✅ Tabla {TableName} existe", _tableName);
                return;
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogInformation("📝 Tabla {TableName} no existe, creando...", _tableName);
            }

            using var ctsCreate = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ctsCreate.CancelAfter(TimeSpan.FromSeconds(5));
            await _dynamoDb.CreateTableAsync(new CreateTableRequest { /* …igual que tienes… */ }, ctsCreate.Token);

            // Espera activa con timeout total
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start) < TimeSpan.FromSeconds(20))
            {
                await Task.Delay(1000, cancellationToken);
                try
                {
                    var st = await _dynamoDb.DescribeTableAsync(_tableName, cancellationToken);
                    if (st.Table.TableStatus == TableStatus.ACTIVE)
                    {
                        _tableInitialized = true;
                        _logger.LogInformation("✅ Tabla {TableName} activa", _tableName);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Verificando estado de tabla…");
                }
            }

            throw new TimeoutException($"Timeout esperando tabla {_tableName}");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task SaveEventAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        // Asegurar que la tabla existe antes de usar
        await EnsureTableExistsAsync(cancellationToken);

        var item = new Dictionary<string, AttributeValue>
        {
            { "ApplicationId", new AttributeValue { S = auditEvent.ApplicationId } },
            { "Timestamp", new AttributeValue { S = auditEvent.Timestamp } },
            { "EventType", new AttributeValue { S = auditEvent.EventType } },
            { "NewState", new AttributeValue { S = auditEvent.NewState } },
            { "Details", new AttributeValue { S = JsonSerializer.Serialize(auditEvent.Details) } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            await _dynamoDb.PutItemAsync(request, cts.Token);

            _logger.LogDebug("✅ Evento guardado en DynamoDB para Application {AppId}",
                auditEvent.ApplicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error guardando evento en DynamoDB");
            throw;
        }
    }

    public async Task<List<AuditEvent>> GetEventsByApplicationIdAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        // Asegurar que la tabla existe antes de usar
        await EnsureTableExistsAsync(cancellationToken);

        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "ApplicationId = :appId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":appId", new AttributeValue { S = applicationId.ToString() } }
            },
            ScanIndexForward = true // Ordenar por timestamp ascendente
        };

        try
        {
            var response = await _dynamoDb.QueryAsync(request, cancellationToken);

            var events = response.Items.Select(item => new AuditEvent
            {
                ApplicationId = item["ApplicationId"].S,
                Timestamp = item["Timestamp"].S,
                EventType = item["EventType"].S,
                NewState = item["NewState"].S,
                Details = JsonSerializer.Deserialize<Dictionary<string, object>>(item["Details"].S)
                    ?? new Dictionary<string, object>()
            }).ToList();

            _logger.LogDebug("📊 Recuperados {Count} eventos para Application {AppId}",
                events.Count, applicationId);

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo eventos de DynamoDB");
            throw;
        }
    }
}