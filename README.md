# EmployeeManagement

Small ASP.NET Core Web API for basic employee CRUD operations.  
The solution is split into layered projects: API, Application, Infrastructure and Domain (the project folder is named `EmployeeManagement.Doamin` in the solution).

## Contents
- `EmployeeManagement.Api` — ASP.NET Core Web API (controllers, program boot)
- `EmployeeManagement.Application` — DTOs, services, application logic
- `EmployeeManagement.Infrastructure` — DB access, repository implementations
- `EmployeeManagement.Doamin` — domain entities and common types (note the project name spelling)

## Requirements
- .NET 8 SDK
- Visual Studio 2022 (or later) or the `dotnet` CLI
- SQL Server reachable by connection string

## Quick start

1. Open the solution
   - In Visual Studio: __File > Open > Project/Solution__ and open the solution in `E:\Mvc-Core\EmployeeManagement` (or open the `.sln` file).
2. Configure the database connection string
   - Recommended: store the connection string in user secrets for the `EmployeeManagement.Api` project.
     - In Visual Studio: right-click the `EmployeeManagement.Api` project > __Manage User Secrets__ and add:
       ```json
       {
         "ConnectionStrings": {
           "DefaultConnection": "Server=YOUR_SERVER;Database=EmployeeManagement;User Id=USER;Password=PASSWORD;TrustServerCertificate=True;"
         }
       }
       ```
     - Or add to `appsettings.Development.json` / environment variables as `ConnectionStrings:DefaultConnection`.
   - The infrastructure layer uses a `DbConnectionFactory` (`IDbConnectionFactory`). The API project registers application and infrastructure services in `Program.cs` with:
     ```csharp
     builder.Services.AddApplication()
                     .AddInfra(builder.Configuration);
     ```
     Ensure your configuration exposes `ConnectionStrings:DefaultConnection` (or the key expected by your `AddInfra` implementation).
3. Create the `Employee` table in your database.
   - Example SQL matching repository queries:
    ```sql
    CREATE TABLE Employee ( Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Email NVARCHAR(200) NOT NULL, Salary DECIMAL(18,2) NOT NULL, HireDate DATETIME2 NOT NULL, IsActive BIT NOT NULL DEFAULT 1, CreateAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL, DeletedAt DATETIME2 NULL, IsDeleted BIT NOT NULL DEFAULT 0 );
    ```
4. Run
   - Visual Studio: select `EmployeeManagement.Api` as startup project and press __Debug > Start Debugging__ or __Debug > Start Without Debugging__.
   - CLI:
     ```bash
     dotnet build
     dotnet run --project EmployeeManagement.Api
     ```
5. Open Swagger UI (development environment): `https://localhost:{port}/swagger` (port printed in console).

## Implementation notes
- The repository layer uses `Microsoft.Data.SqlClient` and raw ADO.NET queries (no EF Core).
- The `EmployeeRepository` performs a soft-delete (updates `IsDeleted` and `DeletedAt`).
- `DbConnectionFactory` implements `IDbConnectionFactory` and accepts the connection string in its constructor; the infrastructure project exposes registration via the `AddInfra` extension used by the API startup.
- Swagger (Swashbuckle) is registered in `Program.cs` and enabled in Development environment.

## Development workflow
- Open solution in Visual Studio 2022, build and run from the IDE or use the `dotnet` CLI as shown above.
- To add local secrets via Visual Studio: right-click project > __Manage User Secrets__.
- To change launch settings (ports, SSL): edit `Properties/launchSettings.json` in `EmployeeManagement.Api`.

## Troubleshooting
- If endpoints return 500: check the connection string and ensure SQL Server is reachable.
- If `GET` returns empty list: verify that `Employee` rows are not soft deleted (`IsDeleted = 0`).
- Use the Output window in Visual Studio: __View > Output__ and select the appropriate pane to inspect build/runtime logs.

## Contributing
- Follow the existing layered architecture: controllers → services → repositories.
- Keep SQL parameterized (see `EmployeeRepository`) to avoid SQL injection.
