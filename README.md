# Global Logistics Management System (GLMS)

## Overview
The **Global Logistics Management System (GLMS)** is an ASP.NET Core MVC web application developed as an enterprise-style prototype for managing logistics clients, contracts, and service requests.

The system was designed to replace a fragmented manual process made up of spreadsheets, emails, and phone calls with a centralized digital solution. It focuses on core business workflows such as:

- Managing clients
- Managing freight/service contracts
- Uploading and downloading signed agreement PDFs
- Creating service requests against valid contracts
- Preventing invalid requests against expired or on-hold contracts
- Converting foreign currency amounts into **ZAR**
- Applying authentication and role-based access control
- Demonstrating a clean **n-tier architecture**

This project was developed as a **Part 2 MVC monolith prototype**, with a layered design that supports later refactoring into a service-oriented/API-based architecture.

---

## Main Features

### Client Management
- Create, edit, view, and delete client records
- Store company and contact information
- Organize contracts per client

### Contract Management
- Create and manage contracts linked to clients
- Track contract status:
  - Draft
  - Active
  - On Hold
  - Expired
- Store service level information
- Filter/search contracts by:
  - status
  - date range
  - client
- View contract details with related data

### Signed Agreement PDF Handling
- Upload a signed contract agreement as a **PDF**
- Save file metadata and file path
- Download the uploaded agreement from the UI
- Restrict uploads to valid file types

### Service Request Processing
- Create service requests linked to a contract
- Capture:
  - description
  - original amount
  - original currency
  - exchange rate to ZAR
  - converted amount in ZAR
  - service request status
- Enforce workflow/business rules:
  - service requests **can only be created for valid active contracts**
  - service requests are **blocked** if the contract is **On Hold** or **Expired**

### Currency Conversion
- Supports converting foreign currencies to **ZAR**
- Uses an external currency API to obtain exchange rates
- Stores both the exchange rate used and the converted ZAR value
- Link to API used: https://frankfurter.dev/

### Authentication and Authorization
- Uses **ASP.NET Core Identity** for secure user storage, password hashing, cookies, and roles
- Uses custom GLMS MVC account pages instead of the scaffolded Identity UI
- Supports role-based access
- Includes a seeded/demo admin account for marking and demonstration

### Layered Architecture
- Database layer
- Repository layer
- Service layer
- Controller layer
- View layer

### Testing
- Unit tests for selected repository and service logic
- Business rule validation tests
- CI pipeline support for automatic test execution via GitHub Actions

---

## Technologies Used

- **ASP.NET Core MVC**
- **C#**
- **.NET**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Core Identity**
- **Bootstrap**
- **xUnit** for testing
- **GitHub Actions** for CI
- **Frankfurter API** for currency exchange rates

---

## Running With Docker Compose

Docker Compose is kept in the `GLMS-Stack/` folder:

```text
GLMS-Stack/
|-- GLMS-Stack.dcproj
`-- docker-compose.yml
```

In Visual Studio:

1. Make sure Docker Desktop is running.
2. Open `GLMS-POE.slnx`.
3. In Solution Explorer, right-click `GLMS-Stack` and choose **Set as Startup Project**.
4. Press **Run** / **Docker Compose**.

The Compose stack starts:

- `sql-server-db`: SQL Server 2022 on host port `1433`
- `backend-api`: API container on host port `8080`
- `frontend-web`: MVC web app on host port `8082`

The Compose project is named `glms-stack`. Container names are managed by Docker Compose instead of being hard-coded, so Docker Desktop may show names such as `glms-stack-frontend-web-1`. This lets Visual Studio and Docker Compose stop/recreate the stack cleanly.

The web app uses the Docker SQL Server connection string from `GLMS-Stack/docker-compose.yml`. On startup, EF Core runs the migrations against `GLMSDb`, and the default admin account is seeded.

If any containers remain running after stopping Visual Studio, stop the full stack from the repository root with:

```powershell
docker compose -f GLMS-Stack/docker-compose.yml down
```

The root `.dockerignore` intentionally stays at the repository root because the Docker build context is the whole repo. Docker only applies `.dockerignore` from the build context root.

---

## Lecturer / Demo Login Details

> **For lecturer/demo access only**

| Role | Email / Username | Password |
|------|------------------|----------|
| Admin | `admin@gmail.com` | `Admin1234!` |

If the seeded account is not available after restoring the project, please ensure the database has been created correctly, migrations have been applied, and any seed/admin initialization logic has run.

---

## N-Tier Architecture

| Layer | Responsibility | GLMS Example |
|------|----------------|--------------|
| **Database** | Stores persistent application data | SQL Server tables for Clients, Contracts, ContractDocuments, ServiceRequests, statuses, and Identity tables |
| **Repositories** | Handles data access and database queries | `ClientRepository`, `ContractRepository`, `ServiceRequestRepository` |
| **Services** | Contains business logic and workflow rules | `ContractService`, `ServiceRequestService`, currency conversion logic, validation rules |
| **Controllers** | Handles HTTP requests and delegates work to services | `ClientsController`, `ContractsController`, `ServiceRequestsController`, `HomeController` |
| **Views** | Presents data to the user through the UI | Razor Views for lists, details pages, forms, dashboards, and file upload/download UI |

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
- Copilot was used to assist with tests.

---

## Project Structure

```text
GLMS/
├── Controllers/
│   ├── HomeController.cs
│   ├── ClientsController.cs
│   ├── ContractsController.cs
│   └── ServiceRequestsController.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Client.cs
│   ├── Contract.cs
│   ├── ContractStatus.cs
│   ├── ContractDocument.cs
│   ├── ServiceRequest.cs
│   └── ServiceRequestStatus.cs
│
├── Repositories/
│   ├── Repository.cs
│   ├── ClientRepository.cs
│   ├── ContractRepository.cs
│   ├── ContractDocumentRepository.cs
│   ├── ServiceRequestRepository.cs
│   ├── ContractStatusRepository.cs
│   └── ServiceRequestStatusRepository.cs
│
├── Services/
│   ├── ClientService.cs
│   ├── ContractService.cs
│   ├── ContractDocumentService.cs
│   ├── ServiceRequestService.cs
│   ├── LookupService.cs
│   └── CurrencyExchangeService.cs
│
├── ViewModels/
│   ├── DashboardViewModel.cs
│   ├── ContractFilterViewModel.cs
│   ├── ContractFormViewModel.cs
│   └── ServiceRequestFormViewModel.cs
│
├── Views/
│   ├── Home/
│   ├── Clients/
│   ├── Contracts/
│   ├── ServiceRequests/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── uploads/
│
├── Migrations/
├── Tests/
└── README.md
