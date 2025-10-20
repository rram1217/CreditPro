# CreditPro Microservicio

## Descripción
Microservicio para la gestión de solicitudes de crédito con persistencia híbrida:
- **PostgreSQL**: Datos transaccionales de solicitudes
- **DynamoDB**: Auditoría y registro de eventos inmutables

## Arquitectura

### Clean Architecture con Carpetas
Este proyecto implementa Clean Architecture en un **único proyecto** organizado por carpetas:

```
CreditPro/
├── Domain/                      # Capa de Dominio
│   ├── Entities/
│   │   ├── CreditApplication.cs
│   │   └── AuditEvent.cs
│   ├── Enums/
│   │   └── CreditApplicationStatus.cs
│   └── Exceptions/
│       ├── DomainException.cs
│       └── NotFoundException.cs
│
├── Application/                 # Capa de Aplicación
│   ├── Interfaces/
│   │   ├── ICreditApplicationRepository.cs
│   │   └── IAuditEventRepository.cs
│   ├── DTOs/
│   │   ├── CreateCreditApplicationRequest.cs
│   │   ├── CreateCreditApplicationResponse.cs
│   │   ├── UpdateStatusRequest.cs
│   │   ├── UpdateStatusResponse.cs
│   │   └── GetApplicationResponse.cs
│   └── UseCases/
│       ├── CreateCreditApplicationUseCase.cs
│       ├── UpdateApplicationStatusUseCase.cs
│       └── GetApplicationWithHistoryUseCase.cs
│
├── Infrastructure/              # Capa de Infraestructura
│   ├── Persistence/
│   │   └── ApplicationDbContext.cs
│   └── Repositories/
│       ├── CreditApplicationRepository.cs
│       └── DynamoDbAuditEventRepository.cs
│
├── Presentation/                # Capa de Presentación
│   └── Controllers/
│       ├── CreditApplicationsController.cs
│       └── HealthController.cs
│
├── Migrations/                  # Migraciones de EF Core
│   └── 20251019000000_InitialCreate.cs
│
├── Program.cs                   # Punto de entrada
├── appsettings.json
└── CreditPro.csproj

CreditPro.Tests/                 # Proyecto de Tests
├── Domain/
│   └── CreditApplicationTests.cs
└── Application/
    ├── CreateCreditApplicationUseCaseTests.cs
    └── UpdateApplicationStatusUseCaseTests.cs
```

**Ventajas de esta estructura:**
- ✅ Simplicidad: Un solo proyecto en lugar de 4-5
- ✅ Mantiene Clean Architecture mediante carpetas
- ✅ Fácil de navegar y entender
- ✅ Menos overhead de configuración
- ✅ Ideal para microservicios pequeños a medianos

## Prerequisitos

- .NET 8 SDK
- Docker y Docker Compose
- PostgreSQL 16 (o usar Docker)
- DynamoDB Local (incluido en Docker Compose)

## Configuración y Ejecución

### Opción 1: Con Docker Compose (Recomendado)

```bash
# 1. Clonar repositorio
git clone https://github.com/tu-usuario/creditpro.git
cd creditpro

# 2. Iniciar servicios (PostgreSQL + DynamoDB)
docker-compose up -d

# 3. Esperar 10 segundos para que las bases estén listas

# 4. Aplicar migraciones
cd CreditPro
dotnet ef database update

# 5. Ejecutar la aplicación
dotnet run
```

La API estará disponible en: `https://localhost:7001`  
Swagger UI: `https://localhost:7001/swagger`

### Opción 2: Sin Docker (Manual)

```bash
# 1. Instalar y configurar PostgreSQL localmente
# 2. Instalar y configurar DynamoDB Local

# 3. Configurar connection string en appsettings.json

# 4. Aplicar migraciones
cd CreditPro
dotnet ef database update

# 5. Ejecutar
dotnet run
```

### Opción 3: Docker para toda la aplicación

```bash
# 1. Construir imagen
docker build -t creditpro:latest -f ../Dockerfile .

# 2. Iniciar todo con docker-compose
docker-compose up -d
```

## Endpoints de la API

### 1. Crear Solicitud de Crédito

```bash
curl -X POST https://localhost:7001/api/credit-applications \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST-12345",
    "creditAmount": 50000.00,
    "applicationDate": "2025-10-19T10:30:00Z",
    "collateralDescription": "Vehículo modelo 2020"
  }'
```

**Respuesta (201 Created):**
```json
{
  "applicationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "customerId": "CUST-12345",
  "creditAmount": 50000.00,
  "applicationDate": "2025-10-19T10:30:00Z",
  "status": "Recibida",
  "collateralDescription": "Vehículo modelo 2020"
}
```

### 2. Actualizar Estado de la Solicitud

```bash
curl -X PATCH https://localhost:7001/api/credit-applications/{applicationId}/status \
  -H "Content-Type: application/json" \
  -d '{
    "newStatus": "Aprobada",
    "notes": "Cliente con buen historial crediticio"
  }'
```

**Estados válidos:** `Aprobada`, `Rechazada`, `EnAnalisis`

### 3. Obtener Solicitud con Historial

```bash
curl -X GET https://localhost:7001/api/credit-applications/{applicationId}
```

**Respuesta (200 OK):**
```json
{
  "application": {
    "applicationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "customerId": "CUST-12345",
    "creditAmount": 50000.00,
    "applicationDate": "2025-10-19T10:30:00Z",
    "status": "Aprobada",
    "collateralDescription": "Vehículo modelo 2020"
  },
  "auditHistory": [
    {
      "applicationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "timestamp": "2025-10-19T10:30:00.000Z",
      "eventType": "Creación",
      "newState": "Recibida",
      "details": {
        "creditAmount": 50000.0,
        "customerId": "CUST-12345"
      }
    },
    {
      "applicationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "timestamp": "2025-10-19T11:15:00.000Z",
      "eventType": "Actualización de Estado",
      "newState": "Aprobada",
      "details": {
        "previousStatus": "Recibida",
        "notes": "Cliente con buen historial crediticio"
      }
    }
  ]
}
```

## Validaciones

### Validación de Monto de Crédito
- **Mínimo**: 1,001 (mayor a 1,000)
- **Máximo**: 149,999 (menor a 150,000)

**Ejemplo de error:**
```json
{
  "error": "Credit amount must be greater than 1,000 and less than 150,000"
}
```

### Estados Válidos
- `Recibida` (estado inicial automático)
- `EnAnalisis`
- `Aprobada`
- `Rechazada`

## Pruebas

### Ejecutar todos los tests

```bash
cd CreditPro.Tests
dotnet test
```

### Ejecutar con cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Tests incluidos

- ✅ Validación de montos (límites)
- ✅ Validación de Customer ID
- ✅ Creación de solicitudes
- ✅ Actualización de estados
- ✅ Generación de eventos de auditoría
- ✅ Manejo de excepciones

## Estructura de Base de Datos

### PostgreSQL - Tabla credit_applications

```sql
CREATE TABLE credit_applications (
    application_id UUID PRIMARY KEY,
    customer_id VARCHAR(100) NOT NULL,
    credit_amount DECIMAL(18,2) NOT NULL,
    application_date TIMESTAMP NOT NULL,
    status VARCHAR(50) NOT NULL,
    collateral_description TEXT,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_customer_id ON credit_applications(customer_id);
CREATE INDEX idx_status ON credit_applications(status);
```

### DynamoDB - Tabla CreditProAuditEvents

```
Partition Key: ApplicationId (String)
Sort Key: Timestamp (String, ISO 8601)

Attributes:
- EventType: String
- NewState: String
- Details: JSON
```

## Variables de Entorno

Configurar en `appsettings.json` o como variables de entorno:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=creditprodb;Username=postgres;Password=postgres123"
  },
  "AWS": {
    "ServiceURL": "http://localhost:8000",
    "Region": "us-east-1",
    "TableName": "CreditProAuditEvents"
  }
}
```

## Docker Compose

El archivo `docker-compose.yml` incluye:

```yaml
services:
  postgres:        # PostgreSQL en puerto 5432
  dynamodb-local:  # DynamoDB Local en puerto 8000
  dynamodb-admin:  # Admin UI en puerto 8001
```

**Comandos útiles:**
```bash
# Iniciar servicios
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar servicios
docker-compose down

# Parar y eliminar volúmenes
docker-compose down -v
```

## Migraciones

### Crear nueva migración

```bash
cd CreditPro
dotnet ef migrations add MigrationName
```

### Aplicar migraciones

```bash
dotnet ef database update
```

### Revertir migración

```bash
dotnet ef database update PreviousMigrationName
```

## Desarrollo

### Ejecutar con Hot Reload

```bash
cd CreditPro
dotnet watch run
```

### Formato de código

```bash
dotnet format
```

### Limpiar proyecto

```bash
dotnet clean
rm -rf bin obj
```

## Decisiones de Arquitectura

### ¿Por qué un solo proyecto?

Para microservicios pequeños a medianos, un solo proyecto con carpetas ofrece:

1. **Simplicidad**: Menos overhead de configuración
2. **Velocidad de desarrollo**: Menos cambios entre proyectos
3. **Compilación más rápida**: Un solo proyecto para compilar
4. **Despliegue simple**: Un solo artefacto
5. **Mantiene Clean Architecture**: La separación por carpetas es suficiente

### ¿Cuándo usar proyectos múltiples?

Considera proyectos separados cuando:
- El equipo es grande (>5 desarrolladores)
- Necesitas compartir Domain/Application entre múltiples APIs
- El proyecto tiene >50,000 líneas de código
- Requieres control granular de dependencias

### Flujo de Dependencias

```
Presentation → Application → Domain
      ↓
Infrastructure → Application → Domain
```

**Reglas:**
- Domain no depende de nada
- Application solo depende de Domain
- Infrastructure depende de Application y Domain
- Presentation depende de Application

## Servicios Disponibles

| Servicio | URL | Descripción |
|----------|-----|-------------|
| API | https://localhost:7001 | API REST |
| Swagger | https://localhost:7001/swagger | Documentación interactiva |
| PostgreSQL | localhost:5432 | Base de datos transaccional |
| DynamoDB | localhost:8000 | Base de datos de auditoría |
| DynamoDB Admin | http://localhost:8001 | UI para DynamoDB |

## Troubleshooting

### Error: Puerto ya en uso

```bash
# Encontrar proceso usando el puerto
lsof -i :5432  # PostgreSQL
lsof -i :8000  # DynamoDB
lsof -i :7001  # API

# Matar proceso
kill -9 <PID>
```

### Error: No se puede conectar a PostgreSQL

```bash
# Verificar que el contenedor esté corriendo
docker ps | grep postgres

# Ver logs
docker logs creditpro-postgres

# Reiniciar
docker-compose restart postgres
```

### Error: Migraciones pendientes

```bash
cd CreditPro
dotnet ef database update
```

## Contribución

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -am 'Agrega nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Crea un Pull Request

## Licencia

MIT License

## Contacto

- Desarrollador: Rafael Ricardo Alvarez Mendieta
- Email: rafael.ricardoam@gmail.com
- GitHub: rram1217