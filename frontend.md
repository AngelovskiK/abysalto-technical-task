# Frontend — Basket App

## Architecture

The frontend is a **React + Vite single-page application** that consumes the Basket API. It separates concerns across UI components, page composition, stateful hooks, and API services.

```
frontend/
├── src/
│   ├── components/              ← Reusable UI blocks (cart, product, common)
│   ├── context/                 ← Auth and toast providers
│   ├── hooks/                   ← App-specific state and cart workflows
│   ├── layout/                  ← Shared application shell/navigation
│   ├── pages/                   ← Route-level screens (home, login, cart)
│   ├── services/                ← HTTP client and API integrations
│   ├── types/                   ← Shared TypeScript contracts
│   ├── App.tsx                  ← Route tree + auth guards
│   └── main.tsx                 ← App bootstrap and providers
└── public/                      ← Static assets
```

### Dependency flow (innermost <- outermost)

```
types <- services <- hooks/context <- components/pages <- App/layout <- main
```

Services are intentionally framework-light and do not depend on React components.

---

## Technology Choices

| Concern | Package / Technology | Rationale |
|---|---|---|
| Framework | `react` + `react-dom` | Component-based SPA architecture with mature ecosystem |
| Build/dev server | `vite` + `@vitejs/plugin-react` | Fast startup/HMR and simple TypeScript-friendly setup |
| Routing | `react-router-dom` | Declarative client-side routing and protected route support |
| Server state | `@tanstack/react-query` | Query caching, retries, mutation lifecycle, and invalidation |
| Styling | `tailwindcss` + `postcss` + `autoprefixer` | Utility-first styling with predictable responsive design |
| Language | `typescript` | Strong typing across UI, API payloads, and domain models |

---

## Local Development

### Prerequisites

- [Node.js 20+](https://nodejs.org/)
- npm (bundled with Node.js)
- Basket API running locally (for full end-to-end flows)

### Install dependencies

From the repo root:

```bash
cd frontend
npm install
```

### Run the frontend

```bash
cd frontend
npm run dev
```

Frontend app: http://localhost:5173

The Vite dev server is configured with a dedicated port and strict mode:

- `port: 5173`
- `strictPort: true`

If port `5173` is in use, Vite will fail fast instead of picking a random port.

### API integration (Development)

The app can call the backend in two supported ways:

1. Through Vite proxy (default)
2. By setting an explicit API base URL

| Variable | Purpose | Default |
|---|---|---|
| `VITE_API_PROXY_TARGET` | Proxy target for `/api` during `npm run dev` | `https://localhost:7054` |
| `VITE_API_BASE_URL` | Absolute base URL used by `services/client.ts` | empty (relative `/api` paths) |

When `VITE_API_BASE_URL` is empty, requests like `/api/cart` go through the dev proxy to `https://localhost:7054`.

### Related local URLs

- Basket API: https://localhost:7054
- Scalar docs: https://localhost:7054/scalar
- OpenTelemetry UI (Aspire Dashboard): http://localhost:18888

### Build for production

```bash
cd frontend
npm run build
npm run preview
```
