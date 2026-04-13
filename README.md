# Integration Bus

Multi-tenant Integration Bus with resilience patterns, built with .NET 8.

## Stack

- .NET 8 / C#
- MSTest
- Polly v8 (Retry, Circuit Breaker)
- System.Text.Json (Streaming)
- Swashbuckle (Swagger/OpenAPI)

## Project Structure

```
src/
  IntegrationBus.Api/          Web API (composition root, middleware, controllers)
  IntegrationBus.Core/         Interfaces and models (no external dependencies)
  IntegrationBus.Services/     Service implementations
tests/
  IntegrationBus.Tests/        Unit tests (MSTest)
```

## Features

### Part 1 - Core Foundation
- **TenantService**: Simulated user retrieval per tenant
- **Tenant Middleware**: Extracts `X-Tenant-ID` header into a scoped `ITenantContext`
- **Swagger**: Global `X-Tenant-ID` header parameter via operation filter

### Part 2 - Integration Bus
- **JSON Streaming**: High-volume order ingestion (100MB+) with validation hooks
- **Dynamic Filtering**: Runtime filter construction using expression trees
- **Resilient Dispatcher**: External API calls with retry (exponential backoff), circuit breaker, 5s timeout, and concurrency throttling (max 5)

## Getting Started

```bash
# Build
dotnet build

# Run API
dotnet run --project src/IntegrationBus.Api

# Run tests
dotnet test
```

## External API Configuration

The dispatcher sends filtered orders to an external provider API. The base URL is configurable via `appsettings`:

```json
"ExternalApi": {
  "BaseUrl": "https://your-api.com"
}
```

**In development** (`appsettings.Development.json`), the URL points to `http://localhost:5083` by default, which routes requests to the built-in mock endpoint `POST /mock/orders`. This allows the full pipeline to run locally without any external dependency.

To use a real external API, update `appsettings.Development.json` (or set the environment variable `ExternalApi__BaseUrl`) before running:

```json
"ExternalApi": {
  "BaseUrl": "https://your-real-api.com"
}
```

The mock endpoint at `POST /mock/orders` will remain available but won't be called.
