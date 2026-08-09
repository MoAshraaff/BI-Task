# BITask — Secure ASP.NET Core Web API (Microservices)

A small microservice-based system built on ASP.NET Core 8 that demonstrates:

- **RESTful CRUD** operations
- **OData v4** endpoints ($filter, $select, $orderby, $top, $skip, $count, $expand)
- **Custom middleware** (exception handling, correlation IDs, request logging)
- **JWT authentication & role-based authorization**
- **Microservice architecture** (independently runnable services + API gateway)

## Architecture

```
                        ┌─────────────────────┐
                        │   Gateway (YARP)     │  http://localhost:5000
                        │  reverse proxy +      │
                        │  shared middleware    │
                        └──────────┬───────────┘
                    ┌──────────────┴──────────────┐
                    │                              │
         ┌──────────▼─────────┐         ┌──────────▼──────────┐
         │   AuthService        │         │   ProductService      │
         │  http://localhost:5001 │       │  http://localhost:5002  │
         │  - register/login     │         │  - REST CRUD           │
         │  - issues JWTs        │         │  - OData /odata/Products│
         │  - SQLite (users)     │         │  - SQLite (products)   │
         └──────────────────────┘         └────────────────────────┘
                    │                              │
                    └──────────────┬───────────────┘
                          ┌─────────▼─────────┐
                          │  BITask.Shared      │  (class library, referenced by all 3)
                          │  - JwtSettings       │
                          │  - AddSharedJwtAuth  │
                          │  - custom middleware │
                          └─────────────────────┘
```

Each service is an independently deployable ASP.NET Core project with its own SQLite database.
`AuthService` is the identity provider: it issues JWTs signed with a shared secret. Every other
service trusts tokens signed with that same secret (see `Jwt` section in each `appsettings.json`)
— in a real deployment that secret would live in a secret manager / environment variable, not in
source control.

## Projects

| Project | Purpose |
|---|---|
| `src/Shared/BITask.Shared` | Common JWT auth wiring + custom middleware, shared by every service |
| `src/Services/AuthService` | User registration/login, JWT issuance |
| `src/Services/ProductService` | Product REST CRUD + OData, JWT-protected |
| `src/Gateway` | YARP reverse proxy fronting both services on one port |

## Custom middleware (`src/Shared/Middleware`)

- **`ExceptionHandlingMiddleware`** — catches unhandled exceptions anywhere downstream and returns
  a consistent JSON error payload with the right status code (never leaks stack traces outside Development).
- **`CorrelationIdMiddleware`** — stamps every request/response with `X-Correlation-Id` (reuses an
  inbound id so a call can be traced across the gateway and both microservices).
- **`RequestLoggingMiddleware`** — logs method, path, status code and elapsed time for every request.

All three are registered identically in the Gateway, AuthService, and ProductService pipelines via
`app.UseCustomExceptionHandling()`, `app.UseCorrelationId()`, `app.UseRequestLogging()`.

## Security

- Passwords hashed with ASP.NET Core Identity's `PasswordHasher<T>` (PBKDF2), never stored in plain text.
- JWTs signed with HMAC-SHA256, validated on issuer, audience, lifetime and signature.
- `[Authorize]` on every Products endpoint; `[Authorize(Roles = "Admin")]` on create/update/delete.
- A seeded default admin (`admin` / `Admin@123`) exists so protected endpoints can be exercised immediately — **change or remove this in any real deployment.**

## Running locally (without Docker)

Requires the .NET 8 SDK.

```bash
dotnet build BITask.sln
```

Run each service in its own terminal:

```bash
dotnet run --project src/Services/AuthService/AuthService.csproj --urls http://localhost:5001
dotnet run --project src/Services/ProductService/ProductService.csproj --urls http://localhost:5002
dotnet run --project src/Gateway/Gateway.csproj --urls http://localhost:5000
```

Swagger UI: `http://localhost:5001/swagger` and `http://localhost:5002/swagger`.

All calls can also go through the gateway on port **5000** (`/api/auth/*`, `/api/products/*`, `/odata/*`).

## Running with Docker Compose

```bash
docker compose up --build
```

This builds and runs all three services (`authservice:5001`, `productservice:5002`, `gateway:5000`) on a shared Docker network, with the gateway pointed at the container hostnames.

## Sample requests

**Login (get a JWT):**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

**Register a new user:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","email":"alice@example.com","password":"Alice@123"}'
```

**REST CRUD (requires `Authorization: Bearer <token>`):**
```bash
curl http://localhost:5000/api/products -H "Authorization: Bearer $TOKEN"
curl http://localhost:5000/api/products/1 -H "Authorization: Bearer $TOKEN"
curl -X POST http://localhost:5000/api/products -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Webcam","description":"1080p","category":"Accessories","price":29.99,"stock":50}'
curl -X PUT http://localhost:5000/api/products/1 -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Wireless Mouse Pro","description":"updated","category":"Accessories","price":24.99,"stock":140}'
curl -X DELETE http://localhost:5000/api/products/1 -H "Authorization: Bearer $TOKEN"
```
(create/update/delete require the `Admin` role — the seeded `admin` account has it; users created via `/register` get the `User` role and can only read.)

**OData:**
```bash
curl "http://localhost:5000/odata/Products?\$filter=Price gt 30&\$orderby=Price desc&\$select=Id,Name,Price" -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5000/odata/Products(1)" -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5000/odata/Products/\$count" -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5000/odata/\$metadata"
```

**Health checks:**
```bash
curl http://localhost:5000/health/auth
curl http://localhost:5000/health/products
```

## Notes / things to change before production

- Move `Jwt:Secret` out of `appsettings.json` into environment variables / a secret manager (e.g. Azure Key Vault, AWS Secrets Manager, or `dotnet user-secrets` for local dev), and use different signing keys per environment.
- SQLite is used for zero-setup local persistence; swap `UseSqlite` for `UseSqlServer`/`UseNpgsql` in production.
- Enable HTTPS redirection/HSTS behind a real reverse proxy or load balancer with TLS termination.
- Add rate limiting and refresh-token rotation for a production-grade auth flow.
