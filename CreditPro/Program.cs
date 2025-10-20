using Amazon.DynamoDBv2;
using Amazon.Runtime;
using CreditPro.Application.Interfaces;
using CreditPro.Application.UseCases;
using CreditPro.Infrastructure.Persistence;
using CreditPro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CreditPro API",
        Description = @"
## Microservicio de Gestión de Solicitudes de Crédito

### Características principales:
- ✅ Creación de solicitudes con validación de monto (1,001 - 149,999)
- ✅ Actualización de estados (Recibida, EnAnalisis, Aprobada, Rechazada)
- ✅ Auditoría completa en DynamoDB
- ✅ Historial inmutable de eventos

### Arquitectura:
- **PostgreSQL**: Datos transaccionales
- **DynamoDB**: Eventos de auditoría
- **Clean Architecture**: Separación por carpetas

### Base de datos:
- **PostgreSQL**: `creditprodb` en puerto 5432
- **DynamoDB**: `CreditProAuditEvents` en puerto 8000
",
        Contact = new OpenApiContact
        {
            Name = "Rafael Alvarez",
            Email = "rafael.ricardoam@gmail.com",
            Url = new Uri("https://github.com/rram1217/creditpro")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Agregar comentarios XML para documentación
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Habilitar anotaciones
    options.EnableAnnotations();

    // Ordenar acciones por nombre
    options.OrderActionsBy(apiDesc => apiDesc.RelativePath);
});

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure DynamoDB
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var serviceUrl = cfg["AWS:ServiceURL"];           // debe ser http://dynamodb-local:8000 en Docker
    var region = cfg["AWS:Region"] ?? "us-east-1";

    var ddbConfig = new AmazonDynamoDBConfig
    {
        ServiceURL = serviceUrl,
        UseHttp = true,
        AuthenticationRegion = region,
        Timeout = TimeSpan.FromSeconds(5)        // <— importante
        //ReadWriteTimeout = TimeSpan.FromSeconds(5)
    };

    var creds = new BasicAWSCredentials("local", "local");
    return new AmazonDynamoDBClient(creds, ddbConfig);
});

// Register repositories
builder.Services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();
builder.Services.AddScoped<IAuditEventRepository, DynamoDbAuditEventRepository>();

// Register use cases
builder.Services.AddScoped<CreateCreditApplicationUseCase>();
builder.Services.AddScoped<UpdateApplicationStatusUseCase>();
builder.Services.AddScoped<GetApplicationWithHistoryUseCase>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Log de configuración de DynamoDB
var cfg = app.Services.GetRequiredService<IConfiguration>();
app.Logger.LogInformation("🔧 DynamoDB configurado - ServiceURL: {Url}, Region: {Region}",
    cfg["AWS:ServiceURL"] ?? "http://localhost:8000 (fallback)",
    cfg["AWS:Region"] ?? "us-east-1");

app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var dynamo = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    try
    {
        var resp = await dynamo.ListTablesAsync();
        Console.WriteLine($"[DynamoDB OK] Tablas: {string.Join(", ", resp.TableNames)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DynamoDB ERROR] {ex.Message}");
    }
});


// Configure Swagger (siempre habilitado para facilitar pruebas)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CreditPro API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "CreditPro API Documentation";
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelExpandDepth(2);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.EnableDeepLinking();
    options.DisplayRequestDuration();
    options.EnableFilter();
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Apply migrations on startup con retry logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(3);

    logger.LogInformation("🔄 Aplicando migraciones de base de datos...");

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("✅ Migraciones aplicadas exitosamente");
            break;
        }
        catch (Exception ex)
        {
            if (attempt == maxRetries)
            {
                logger.LogError(ex, "❌ No se pudieron aplicar las migraciones después de {MaxRetries} intentos", maxRetries);
                throw;
            }

            logger.LogWarning("⚠️  Base de datos no lista. Reintento {Attempt}/{Max} en {Delay}s...",
                attempt, maxRetries, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}

app.Logger.LogInformation("🚀 CreditPro API iniciada - Swagger disponible en /swagger");
app.Run();