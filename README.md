# Abysalto Technical Task — Global Online Retail Platform

## Documentation

| Document | Description |
|---|---|
| [ARCHITECTURE.md](ARCHITECTURE.md) | High-level system design: architecture diagram, scaling strategy, security, monitoring, CI/CD |
| [backend.md](backend.md) | Backend service design: project structure, technology choices, local dev setup, migrations |
| [frontend.md](frontend.md) | Frontend application design: structure, technology choices, local dev setup |

## Quick Start

```bash
# Start all infrastructure services (PostgreSQL, Redis, Azurite, Aspire Dashboard)
docker compose up -d

# Run the Basket API locally
cd backend/BasketService/src/BasketService.Api
dotnet run

# In a separate terminal, run the frontend app
cd frontend
npm install
npm run dev
```

Frontend app: http://localhost:5173
Basket API: https://localhost:7054
Scalar API docs: https://localhost:7054/scalar
OpenTelemetry UI (Aspire Dashboard): http://localhost:18888