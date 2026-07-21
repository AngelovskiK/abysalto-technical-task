# Abysalto Technical Task — Global Online Retail Platform

## Documentation

| Document | Description |
|---|---|
| [ARCHITECTURE.md](ARCHITECTURE.md) | High-level system design: architecture diagram, scaling strategy, security, monitoring, CI/CD |
| [backend.md](backend.md) | Backend service design: project structure, technology choices, local dev setup, migrations |

## Quick Start

```bash
# Start all infrastructure services (PostgreSQL, Redis, Azurite, Aspire Dashboard)
docker compose up -d

# Run the Basket API locally
cd BasketService/src/BasketService.Api
dotnet run
```

OpenTelemetry UI (Aspire Dashboard): http://localhost:18888