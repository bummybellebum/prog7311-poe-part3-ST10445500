# 🌐 Global Logistics Management System (GLMS)

> **PROG7311 / EAPD7111 - Programming 3A & Enterprise Application Development**


> **Student Number:** ST10445500

---

## 📑 Table of Contents

- [What is GLMS?](#-what-is-glms)
- [How the Project Evolved](#-how-the-project-evolved)
- [Final Architecture](#-final-architecture)
- [Tech Stack](#️-tech-stack)
- [Docker & Containerisation](#-docker--containerisation)
- [Running Locally (Without Docker)](#️-running-locally-without-docker)
- [Testing](#-testing)
- [Default Demo Users](#-default-demo-users)
- [Project Structure](#-project-structure)
- [Key API Endpoints](#-key-api-endpoints)
- [Design Decisions Worth Noting](#-design-decisions-worth-noting)
- [POE Submission Notes](#-poe-submission-notes)

---

## 📋 What is GLMS?

**TechMove Logistics** is a fictional global freight coordinator drowning in spreadsheets, email threads, and manual phone calls. They needed something better - a proper enterprise system to manage clients, contracts, service requests, and international invoicing.

**GLMS** is that system. Built across three POE parts, it evolved from an architecture report → a working prototype → a fully containerised, service-oriented web application. The final product is a real multi-project .NET solution with a decoupled API backend, an MVC frontend, automated tests, and Docker Compose deployment.

The system lets TechMove staff:

- Manage clients, contracts, and signed agreement documents
- Create and track service requests tied to specific contracts
- Block service requests against expired or on-hold contracts automatically
- Convert foreign currency amounts to ZAR in real time via an external API
- Sign in with role-based access (Admin, ContractManager, LogisticsManager)
- Run and test everything with a single Docker Compose command

---

## 🏗️ How the Project Evolved

This project was built in three parts, each building on the last:

| Part | What We Did |
|------|------------|
| **Part 1 - Architecture Report** | Designed the system. Chose an enterprise framework (Zachman/TOGAF), selected GoF design patterns, defined scalability strategies. No code yet - just the blueprint. |
| **Part 2 - Core Prototype** | Built a working ASP.NET Core MVC monolith. Clients, contracts, service requests, PDF uploads, currency conversion, unit tests. Everything in one project. |
| **Part 3 - Modernisation** | Refactored into a proper Service-Oriented Architecture. Split the backend out into its own Web API. Containerised everything with Docker Compose. Added integration tests. |

---

## 🧱 Final Architecture

The final version is a three-tier, service-oriented system:

```
Browser
  │
  ▼
GLMS.Web  (ASP.NET Core MVC Frontend)
  │  calls via HttpClient / JSON
  ▼
GLMS.Api  (ASP.NET Core Web API Backend)
  │  queries via Entity Framework Core
  ▼
SQL Server Database
```

**Why split it up?**
The MVC frontend no longer talks to the database directly. It sends HTTP requests to the API, which owns all the business rules, validation, file handling, and database access. This means the two layers can be deployed, scaled, and updated independently - which is the whole point of a Service-Oriented Architecture.

### Projects in the Solution

| Project | Purpose |
|---------|---------|
| `GLMS.Api` | Web API backend - all database access, business logic, authentication, and JSON endpoints live here. |
| `GLMS.Web` | MVC frontend - Razor views, form handling, and typed `HttpClient` API clients. No SQL here. |
| `GLMS.Tests` | xUnit unit and integration tests. |

---

## 🛠️ Tech Stack

| Layer | Technology | Why We Used It |
|-------|-----------|----------------|
| **Backend Framework** | ASP.NET Core Web API (`.NET 10`) | Industry-standard, performant, strongly typed. The obvious choice for enterprise .NET backends. |
| **Frontend Framework** | ASP.NET Core MVC | Razor views with controller-based routing. Keeps the UI layer clean and separate from data logic. |
| **ORM / Database Access** | Entity Framework Core + SQL Server | Code-first migrations, LINQ queries, and clean entity relationships without writing raw SQL. |
| **Authentication** | ASP.NET Core Identity + JWT Bearer | Identity manages users and roles; JWT tokens let the API authenticate requests without sessions. |
| **API Documentation** | Swagger / OpenAPI (Swashbuckle) | Auto-generates interactive API docs. Makes testing endpoints dead-simple without needing Postman. |
| **Currency Conversion** | Frankfurter API (external) | Free, reliable exchange-rate API. No key required. Used to convert foreign currency amounts to ZAR in real time. |
| **Testing** | xUnit + WebApplicationFactory | xUnit for unit tests; `WebApplicationFactory` spins up the real API in memory for integration tests. |
| **Test Database** | EF Core InMemory Provider | Gives tests a clean, isolated database without needing a running SQL Server instance. |
| **Containerisation** | Docker + Docker Compose | Packages the API, MVC app, and SQL Server into isolated containers that run the same way everywhere. |

---

## 🐳 Docker & Containerisation

### Why Docker?

One of the biggest problems in software development is *"it works on my machine."* A developer's laptop, a lecturer's PC, and a cloud server all have different operating systems, software versions, and configurations - which means code that works in one place can silently break in another.

**Docker solves this** by packaging your application and everything it needs (runtime, dependencies, configuration) into a single **container image**. That image runs identically on any machine that has Docker installed.

**Docker Compose** takes it a step further - it lets you define multiple containers (database, API, frontend) in a single `docker-compose.yml` file and bring the whole stack up with one command.

### Our Docker Setup

```
docker-compose.yml
  ├── glms-db       → SQL Server 2022 database container
  ├── glms-api      → ASP.NET Core Web API (connects to glms-db)
  └── glms-web      → ASP.NET Core MVC frontend (connects to glms-api)
```

The containers communicate using Docker's internal networking - the API doesn't use `localhost` to find the database, it uses the service name `glms-db`. The frontend doesn't use `localhost` to find the API, it uses `http://glms-api:8080/`. This keeps everything portable.

**Persistent data** is handled with Docker volumes:
- `sql-server-data` - keeps the database alive between restarts
- `contract-uploads` - stores uploaded signed agreement PDFs

### Running with Docker (Recommended)

Make sure Docker Desktop is running, then from the repo root:

```powershell
docker compose up --build
```

Then open:
- **MVC Frontend:** `http://localhost:5004`
- **API Swagger UI:** `http://localhost:5053/swagger`

To stop everything:
```powershell
docker compose down
```

To do a full reset (wipes the database and uploaded files):
```powershell
docker compose down -v
```

> ⚠️ The `.env` file supplies secrets like the SA password and JWT key. Do not commit real production secrets. The defaults are for demo/marking purposes only.

---

## ▶️ Running Locally (Without Docker)

If you'd rather run it without Docker, you'll need a local SQL Server instance and the right config values in `appsettings.Development.json`.

**1. Restore and build:**
```powershell
dotnet restore
dotnet build .\GLMS-POE.slnx
```

**2. Run the API** (in one terminal):
```powershell
dotnet run --project .\GLMS.Api\GLMS.Api.csproj
```

**3. Run the MVC frontend** (in another terminal):
```powershell
dotnet run --project .\GLMS.Web\GLMS.Web.csproj
```

Local URLs:
- API: `http://localhost:64083` (+ Swagger at `/swagger`)
- Frontend: `http://localhost:64081`

---

## 🧪 Testing

Tests live in `GLMS.Tests` and use **xUnit**.

```powershell
dotnet test .\GLMS.Tests\GLMS.Tests.csproj
```

**Latest result: ✅ 28/28 tests passing**

### Unit Tests cover:
- Contract service validation logic
- Service request business rules
- Blocking requests against Expired and On Hold contracts
- PDF upload validation (PDF-only, header checks, path safety)
- Currency conversion math and error handling
- Repository filtering behaviour

### Integration Tests cover:
- Real HTTP calls to API endpoints via `WebApplicationFactory`
- Asserting correct HTTP status codes and JSON responses
- End-to-end flows (create → read → verify)

Integration tests use an **InMemory database**, fake authentication, and a fake currency service - so they're fully isolated and repeatable without needing a live SQL Server or the external Frankfurter API.

**Why does automated testing matter?**
In a real DevOps pipeline, these tests run automatically every time code is pushed. If a change breaks an existing feature, the tests catch it before it reaches production. The repo includes a **GitHub Actions CI workflow** (`.github/workflows/CI.yml`) that runs restore, build, and test on every push.

---

## 🔑 Default Demo Users

The seed data creates three demo accounts on first startup:

| Role | Email | Password |
|------|-------|----------|
| `Admin` | `admin@glms.co.za` | `Admin1234!` |
| `ContractManager` | `contracts@glms.co.za` | `User1234!` |
| `LogisticsManager` | `logistics@glms.co.za` | `User1234!` |

These are for **demo and marking purposes only**.

### What each role can do:

| Action | Admin | ContractManager | LogisticsManager |
|--------|:-----:|:---------------:|:----------------:|
| View clients, contracts, service requests | ✅ | ✅ | ✅ |
| Create / edit clients and contracts | ✅ | ✅ | ❌ |
| Upload signed agreement PDFs | ✅ | ✅ | ❌ |
| Update contract statuses | ✅ | ✅ | ❌ |
| Create service requests | ✅ | ❌ | ✅ |
| Update service request statuses | ✅ | ❌ | ✅ |
| Use currency conversion | ✅ | ❌ | ✅ |
| Manage users | ✅ | ❌ | ❌ |
| Delete records | ✅ | ❌ | ❌ |

---

## 📁 Project Structure

```
prog7311-poe-part3-ST10445500/
  GLMS-POE.slnx
  docker-compose.yml
  .env

  GLMS.Api/               ← Web API backend
    Controllers/          ← API endpoints
    Data/
      Repositories/       ← Database query logic
      Seeding/            ← Demo data on startup
    DTOs/                 ← Data shapes in/out of the API
    Models/               ← EF Core entities
    Services/             ← Business rules and validation
    Migrations/           ← EF Core database migrations
    uploads/              ← Uploaded signed agreement PDFs
    Dockerfile

  GLMS.Web/               ← MVC frontend
    ApiClients/           ← HttpClient wrappers for each API area
    Controllers/          ← MVC controllers
    ViewModels/           ← Data shapes for Razor views
    Views/                ← Razor pages
    Dockerfile

  GLMS.Tests/             ← Test project
    UnitTests/
    IntegrationTests/
    Helpers/
```

---

## 🌐 Key API Endpoints

| Method | Route | What it does |
|--------|-------|-------------|
| `POST` | `/api/auth/login` | Log in and get a JWT token |
| `GET` | `/api/clients` | List all clients |
| `GET` | `/api/contracts` | List contracts (supports filtering by status and date) |
| `POST` | `/api/contracts` | Create a new contract |
| `PATCH` | `/api/contracts/{id}/status` | Update contract status |
| `POST` | `/api/contracts/{id}/signed-agreement` | Upload a signed agreement PDF |
| `GET` | `/api/contracts/documents/{id}/download` | Download a signed agreement PDF |
| `GET` | `/api/service-requests` | List service requests |
| `POST` | `/api/service-requests` | Create a service request (validated against contract status) |
| `GET` | `/api/service-requests/exchange-rate` | Get live ZAR exchange rate |
| `GET` | `/api/lookups/contract-statuses` | Get available contract status options |

Swagger UI is available at `/swagger` when the API runs in Development mode.

---

## 💡 Design Decisions Worth Noting

**Why did we start with a monolith (Part 2) before splitting it up (Part 3)?**
Building a monolith first is actually a standard, recommended approach for MVPs. You get all the core logic working and tested in one place before you worry about separation. The refactor in Part 3 was intentional - once we knew the business logic was solid, we separated it cleanly into an API backend and an MVC frontend.

**Why Service-Oriented Architecture?**
It means the frontend and backend can be deployed, scaled, and updated independently. If TechMove suddenly needs to support a mobile app, it just calls the same API. The MVC project is just one possible frontend.

**Why EF Core over raw SQL?**
Code-first migrations mean the database schema stays in sync with the C# models automatically. LINQ queries are type-safe, which reduces bugs. And it's much faster to iterate on during development.

**Why JWT over cookie-based sessions?**
JWTs are stateless - the API doesn't need to store session data. This makes the API easier to scale horizontally (multiple instances don't need to share session state). It also makes Swagger testing straightforward: paste the token in once and all protected endpoints work.

---

## 📝 POE Submission Notes

| Part | Deliverables |
|------|-------------|
| **Part 1** | Architecture report (PDF), framework diagram, UML class diagram for design patterns |
| **Part 2** | GitHub repo, EF Core migrations, test screenshots, demo video |
| **Part 3** | GitHub repo, Dockerfiles, docker-compose.yml, Docker Desktop screenshots, reflection report (PDF), demo video |

> ⚠️ Submission without a GitHub link results in a **5% deduction** per part.

---

*Built for PROG7311 / EAPD7111 - IIE 2026*
