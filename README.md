# Global Logistics Management System (GLMS)

## Overview

The **Global Logistics Management System (GLMS)** is an ASP.NET Core enterprise-style logistics management system developed for TechMove Logistics.

The system replaces a fragmented manual workflow made up of spreadsheets, emails, and phone calls with a centralized digital platform for managing clients, contracts, signed agreements, service requests, and currency conversion.

GLMS was initially developed as an ASP.NET Core MVC monolith prototype and then modernised into a separated **MVC frontend + Web API backend** architecture for Part 3 of the POE. The backend API owns the database and business logic, while the MVC frontend communicates with it using `HttpClient`.

The system focuses on:

- Managing clients
- Managing freight/service contracts
- Uploading and downloading signed agreement PDFs
- Creating service requests against valid contracts
- Blocking invalid service requests against expired or on-hold contracts
- Converting foreign currency amounts into **ZAR**
- Applying authentication and role-based access control
- Demonstrating clean separation of concerns
- Running the full system through Docker Compose

---

## Table of Contents

- [Overview](#overview)
- [Main Features](#main-features)
- [User Roles and Permissions](#user-roles-and-permissions)
- [Architecture](#architecture)
- [API and Frontend Separation](#api-and-frontend-separation)
- [N-Tier Architecture](#n-tier-architecture)
- [Docker Setup](#docker-setup)
- [Demo Login Details](#demo-login-details)
- [Testing](#testing)
- [Technologies Used](#technologies-used)
- [Project Structure](#project-structure)
- [Screenshots](#screenshots)
- [YouTube Video Link](#youtube-video-link)
- [References](#references)

---

## Main Features

### Client Management

- Create, edit, view, and delete client records
- Store client company and contact details
- Store the client region
- Link contracts to specific clients
- View client-related contract information

### Contract Management

- Create and manage contracts linked to clients
- Track contract statuses:
  - Draft
  - Active
  - On Hold
  - Expired
- Store contract start date and end date
- Store service level information
- Filter/search contracts by:
  - status
  - date range
  - client
- View contract details with related client, document, and service request data

### Signed Agreement PDF Handling

- Upload a signed agreement PDF for a contract
- Save uploaded agreement files to the server/container file system
- Store the file path and metadata in the database
- Download uploaded agreements from the UI
- Restrict uploads to valid PDF files
- Prevent invalid file types such as `.exe`

### Service Request Processing

- Create service requests linked to contracts
- Capture:
  - description
  - original amount
  - original currency
  - exchange rate to ZAR
  - converted amount in ZAR
  - service request status
- Enforce business workflow rules:
  - service requests can only be created for **Active** contracts
  - service requests are blocked when the contract is **Expired**
  - service requests are blocked when the contract is **On Hold**

### Currency Conversion

- Converts foreign currency amounts into **ZAR**
- Uses an external currency exchange API
- Stores the exchange rate used during the conversion
- Stores the converted ZAR amount with the service request
- API used: https://frankfurter.dev/

### Authentication and Authorization

- Uses **ASP.NET Core Identity** for secure user storage, password hashing, cookies, and roles
- Uses custom GLMS account/login pages instead of the scaffolded Identity UI
- Supports role-based access control for Admin, Contract Manager, and Logistics Manager users
- Restricts sensitive features based on user role
- Includes seeded/demo accounts for marking and demonstration

### Swagger / OpenAPI

- The Web API includes Swagger/OpenAPI support
- API endpoints can be tested directly through the Swagger UI
- Useful for checking contract, client, service request, authentication, and status endpoints during development

---

## User Roles and Permissions

GLMS uses a simple 3-role system. The roles are designed around the main business areas of the system without adding unnecessary extra users.

### Role Summary

| Role | Purpose |
|------|---------|
| **Admin** | Full system access and demonstration/marker account |
| **ContractManager** | Manages clients, contracts, signed agreements, and contract statuses |
| **LogisticsManager** | Creates and manages service requests against active contracts |

### Permission Matrix

| Feature | Admin | ContractManager | LogisticsManager |
|--------|:-----:|:---------------:|:----------------:|
| View dashboard | Yes | Yes | Yes |
| Manage users/roles | Yes | No | No |
| View clients | Yes | Yes | Yes |
| Create/edit clients | Yes | Yes | No |
| Delete clients | Yes | Optional | No |
| View contracts | Yes | Yes | Yes |
| Filter contracts | Yes | Yes | Yes |
| Create/edit contracts | Yes | Yes | No |
| Change contract status | Yes | Yes | No |
| Upload signed agreement PDFs | Yes | Yes | No |
| Download signed agreement PDFs | Yes | Yes | Yes |
| View service requests | Yes | Yes | Yes |
| Create service requests | Yes | No | Yes |
| Update service request status | Yes | No | Yes |
| Use currency conversion | Yes | No | Yes |
| Access API testing tools in development | Yes | No | No |

### Role Behaviour

#### Admin

The Admin role has full access to the system. This role is mainly used for system administration, lecturer/demo access, and verifying all workflows.

Admins can:

- manage clients
- manage contracts
- upload and download contract PDFs
- filter contracts
- change contract statuses
- create and update service requests
- access all major areas of the system
- manage users/roles if user management is enabled

#### ContractManager

The Contract Manager role is responsible for the contract management side of the business.

Contract Managers can:

- manage clients
- create and edit contracts
- upload signed agreement PDFs
- download signed agreements
- change contract statuses
- view service requests linked to contracts

Contract Managers should not normally create service requests, because that is handled by the logistics team.

#### LogisticsManager

The Logistics Manager role is responsible for service request processing.

Logistics Managers can:

- view clients and contracts
- create service requests
- use currency conversion during service request creation
- update service request statuses
- download contract agreements if needed

Logistics Managers cannot create service requests for contracts that are **Expired** or **On Hold**. This rule is enforced by the backend business logic, not only by the frontend UI.

---

## Architecture

GLMS uses a layered architecture with a separated frontend and backend.

```text
User
 |
 v
GLMS.Web
ASP.NET Core MVC Frontend
 |
 | HttpClient
 v
GLMS.Api
ASP.NET Core Web API Backend
 |
 | Entity Framework Core
 v
SQL Server
```

The backend API is the only project that connects directly to SQL Server. The MVC frontend does not access the database directly.

---

## API and Frontend Separation

### GLMS.Api

The API project is responsible for:

- database access
- Entity Framework Core configuration
- repositories
- business services
- validation rules
- authentication/JWT or Identity-related API logic
- file upload handling
- currency conversion logic
- returning JSON responses
- exposing REST endpoints
- Swagger/OpenAPI documentation

Example API responsibilities:

```text
GET    /api/contracts
POST   /api/contracts
PATCH  /api/contracts/{id}/status
GET    /api/clients
POST   /api/service-requests
GET    /api/currency/usd-zar
```

### GLMS.Web

The MVC frontend is responsible for:

- Razor Views
- user-facing pages
- form handling
- calling the API through `HttpClient`
- displaying validation messages from the API
- storing/authenticating the logged-in user session
- hiding/showing menu items based on role

The MVC project should not contain:

- direct SQL Server access
- `DbContext` usage
- EF Core migrations
- repository classes
- final business-rule enforcement

---

## N-Tier Architecture

| Layer | Responsibility | GLMS Example |
|------|----------------|--------------|
| **Database** | Stores persistent application data | SQL Server tables for Clients, Contracts, ContractDocuments, ServiceRequests, statuses, and Identity tables |
| **Repositories** | Handles data access and database queries | Client, Contract, ContractDocument, and ServiceRequest repositories |
| **Services** | Contains business logic and workflow rules | Contract validation, service request validation, file validation, currency conversion |
| **API Controllers** | Exposes JSON endpoints and delegates work to services | Clients API, Contracts API, Service Requests API |
| **MVC Services** | Calls the backend API from the frontend | `ClientsApiService`, `ContractsApiService`, `ServiceRequestsApiService` |
| **MVC Controllers** | Handles frontend page requests and delegates to MVC services | Clients, Contracts, Service Requests, Account controllers |
| **Views** | Presents data to users | Razor Views for dashboards, lists, details pages, forms, uploads, and downloads |

---

## Docker Setup

The full GLMS system can be run using Docker Compose.

Docker Compose is kept at the repository root:

```text
|-- docker-compose.yml
|-- .env
|-- GLMS-Stack.dcproj
|-- GLMS.Api/
|-- GLMS.Web/
`-- GLMS.Tests/
```

The Docker Compose stack starts three main services:

| Service | Purpose |
|---------|---------|
| **glms-db** | SQL Server database container |
| **glms-api** | ASP.NET Core Web API backend |
| **glms-web** | ASP.NET Core MVC frontend |

### Docker Networking

The containers communicate using Docker's internal network.

The MVC frontend calls the backend API using the API service name, for example:

```text
http://glms-api:8080/
```

The API connects to SQL Server using the database service name, for example:

```text
Server=glms-db,1433;Database=GLMSDb;User Id=sa;Password=...
```

This means the containers do not need to use `localhost` to talk to each other. Inside Docker, each service is reached by its Compose service name.

### Running With Visual Studio

1. Make sure Docker Desktop is running.
2. Open the solution.
3. Set the Docker Compose project as the startup project.
4. Press **Run** / **Docker Compose**.
5. Wait for SQL Server, the API, and the MVC frontend to start.

### Running From Terminal

From the repository root:

```powershell
docker compose up --build
```

To stop and remove the running stack:

```powershell
docker compose down
```

To remove the stack and volumes if the database needs to be reset:

```powershell
docker compose down -v
```

### Docker Notes

- SQL Server runs in its own container.
- The API container connects to SQL Server through Docker networking.
- The MVC frontend container connects to the API through Docker networking.
- Container names may be prefixed by Docker Compose.
- The `.env` file is used for environment-specific values such as database passwords and connection settings.
- The root `.dockerignore` stays at the repository root because the Docker build context is the full repository.
- Migrations are applied to the database during startup or through the configured migration process.

---

## Demo Login Details

> For lecturer/demo access only.

| Role | Email / Username | Password |
|------|------------------|----------|
| Admin | `admin@gmail.com` | `Admin1234!` |
| ContractManager | `contracts@gmail.com` | `Contract1234!` |
| LogisticsManager | `logistics@gmail.com` | `Logistics1234!` |

If these seeded accounts are not available after restoring the project, check that:

- the database was created successfully
- migrations were applied
- the seed logic ran
- the `.env` file contains the expected configuration values

If only the admin account is currently seeded, the ContractManager and LogisticsManager accounts should be added through the seed logic before the final demo.

---

## Testing

GLMS includes automated tests for important business rules and system behaviour.

### Unit Tests

Unit tests cover selected service and validation logic, such as:

- currency calculation
- PDF file validation
- service request validation
- blocked requests for Expired contracts
- blocked requests for On Hold contracts
- repository/service behaviour where appropriate

### Integration Tests

Integration tests are used for the Web API.

Example integration test coverage:

- `GET /api/contracts` returns a successful response
- creating a contract returns the correct status code
- patching a contract status updates the contract
- creating a service request works for an Active contract
- creating a service request fails for an Expired or On Hold contract
- unauthenticated requests are rejected where authorization is required
- users without the required role receive the correct response

### GitHub Actions

The project supports CI through GitHub Actions.

The pipeline can be used to:

- restore dependencies
- build the solution
- run automated tests
- catch breaking changes before submission

---

## Technologies Used

- **ASP.NET Core MVC**
- **ASP.NET Core Web API**
- **C#**
- **.NET**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Core Identity**
- **JWT / role-based authentication**
- **HttpClient**
- **Swagger / OpenAPI**
- **Bootstrap**
- **xUnit**
- **GitHub Actions**
- **Docker**
- **Docker Compose**
- **Frankfurter API**

---

## Project Structure

```text
GLMS/
├── GLMS.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ClientsController.cs
│   │   ├── ContractsController.cs
│   │   ├── ServiceRequestsController.cs
│   │   └── CurrencyController.cs
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── SeedData.cs
│   │
│   ├── Models/
│   │   ├── ApplicationUser.cs
│   │   ├── Client.cs
│   │   ├── Contract.cs
│   │   ├── ContractStatus.cs
│   │   ├── ContractDocument.cs
│   │   ├── ServiceRequest.cs
│   │   └── ServiceRequestStatus.cs
│   │
│   ├── DTOs/
│   ├── Repositories/
│   ├── Services/
│   ├── FileStorage/
│   ├── Migrations/
│   ├── Program.cs
│   └── Dockerfile
│
├── GLMS.Web/
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── ClientsController.cs
│   │   ├── ContractsController.cs
│   │   ├── ServiceRequestsController.cs
│   │   └── AccountController.cs
│   │
│   ├── Services/
│   │   ├── AuthApiService.cs
│   │   ├── ClientsApiService.cs
│   │   ├── ContractsApiService.cs
│   │   ├── ServiceRequestsApiService.cs
│   │   └── CurrencyApiService.cs
│   │
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── Dockerfile
│
├── GLMS.Tests/
│   ├── Unit/
│   └── Integration/
│
├── docker-compose.yml
├── .env
├── .dockerignore
├── GLMS-Stack.dcproj
├── GLMS-POE.slnx
└── README.md
```

---

## Screenshots

<img width="1885" height="854" alt="Screenshot 2026-04-22 142550" src="https://github.com/user-attachments/assets/c9cbed90-0b97-4e24-a8d5-9f2e161a4634" />

<img width="1125" height="466" alt="Screenshot 2026-04-22 142643" src="https://github.com/user-attachments/assets/4ce96a42-c563-4a2d-bf7c-9acbdd5bce44" />

---

## YouTube Video Link

- https://youtu.be/O15G2dXdf_c

---

## References

- wadepickett (2025). Introduction to Identity on ASP.NET Core. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0&utm.
- meaghanlewis (2026). Testing in .NET - .NET. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/dotnet/core/testing/?utm.
- tdykstra (2025). Integration tests in ASP.NET Core. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0&pivots=xunit.
- Frankfurter. (2026). Frankfurter. [online] Available at: https://frankfurter.dev/?utm.
- SamMonoRT (2024). Overview of Entity Framework Core - EF Core. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/ef/core/?utm.
- gewarren (2025). HttpClient guidelines for .NET - .NET. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines?utm.
- Evan Gudmestad (2024). ASP.NET Core MVC Tutorial – Full Course to Build YOUR Passion Project! [online] YouTube. Available at: https://www.youtube.com/watch?v=q9X3SDEZtpw.
- Iulian Oana (2021). Beginners ASP.NET Core Identity Tutorial. [online] YouTube. Available at: https://www.youtube.com/watch?v=5UfJeDcoC1k.
- Copilot was used to assist with selected tests.
