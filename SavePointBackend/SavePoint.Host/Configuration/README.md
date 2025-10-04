# Service Configuration Documentation

This document describes the organized service configuration structure implemented to clean up the `Program.cs` file.

## Overview

The service configuration has been reorganized into separate configuration classes, each responsible for a specific area of the application setup. This improves maintainability, readability, and follows the Single Responsibility Principle.

## Configuration Classes

### `ServiceConfiguration`
**Location**: `SavePoint.Host/Configuration/ServiceConfiguration.cs`
- **Purpose**: Main orchestrator that brings all configuration classes together
- **Key Method**: `AddApplicationServices()` - registers all application services

### `DatabaseConfiguration`
**Location**: `SavePoint.Host/Configuration/DatabaseConfiguration.cs`
- **Purpose**: Configures Entity Framework and database-related services
- **Services Registered**:
  - `ApplicationDbContext`
  - `DatabaseSeederService`
- **Extension Methods**:
  - `AddDatabaseServices()` - registers database services
  - `SeedDatabaseAsync()` - seeds the database with initial data

### `IdentityConfiguration`
**Location**: `SavePoint.Host/Configuration/IdentityConfiguration.cs`
- **Purpose**: Configures ASP.NET Core Identity and authentication
- **Services Registered**:
  - Identity services with custom options
  - Cookie authentication
  - Authorization services
- **Key Features**:
  - Password requirements
  - Lockout settings
  - Cookie configuration
  - API-friendly authentication responses

### `BusinessServicesConfiguration`
**Location**: `SavePoint.Host/Configuration/BusinessServicesConfiguration.cs`
- **Purpose**: Registers business logic services
- **Services Registered**:
  - `IUserService` ? `UserService`
  - `IIGDBImportService` ? `IGDBImportService`
  - `IGameService` ? `GameService`
  - `ILookupService` ? `LookupService`

### `RepositoryConfiguration`
**Location**: `SavePoint.Host/Configuration/RepositoryConfiguration.cs`
- **Purpose**: Registers data access layer repositories
- **Services Registered**:
  - `IGenreRepository` ? `GenreRepository`
  - `IGameRepository` ? `GameRepository`
  - `ICompanyRepository` ? `CompanyRepository`
  - `IPlatfromRepository` ? `PlatfromRepository`
  - `IPopularityRepository` ? `PopularityRepository`

### `MappingConfiguration`
**Location**: `SavePoint.Host/Configuration/MappingConfiguration.cs`
- **Purpose**: Configures AutoMapper with all mapping profiles
- **Profiles Registered**:
  - `GameMappingProfile`
  - `UserMappingProfile`
  - `LookupMappingProfile`

### `CorsConfiguration`
**Location**: `SavePoint.Host/Configuration/CorsConfiguration.cs`
- **Purpose**: Configures Cross-Origin Resource Sharing (CORS)
- **Features**:
  - Allows frontend origins (localhost:5173)
  - Supports credentials for cookie authentication

### `ExternalServicesConfiguration`
**Location**: `SavePoint.Host/Configuration/ExternalServicesConfiguration.cs`
- **Purpose**: Configures external service clients
- **Services Registered**:
  - `IGDBClient` with credentials from configuration

### `WebApiConfiguration`
**Location**: `SavePoint.Host/Configuration/WebApiConfiguration.cs`
- **Purpose**: Configures Web API services and middleware pipeline
- **Services Registered**:
  - Controllers with JSON options
  - Swagger/OpenAPI
- **Pipeline Configuration**:
  - HTTPS redirection
  - CORS
  - Authentication/Authorization
  - Controller routing

## Usage

The new structure significantly simplifies `Program.cs`:

```csharp
using SavePoint.Host.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register all application services using the organized configuration classes
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Seed the database
await app.SeedDatabaseAsync();

// Configure the web API pipeline
app.ConfigureWebApiPipeline();

app.Run();
```

## Benefits

1. **Separation of Concerns**: Each configuration class handles a specific area
2. **Maintainability**: Changes to specific configurations are isolated
3. **Readability**: `Program.cs` is now clean and easy to understand
4. **Testability**: Individual configuration classes can be tested separately
5. **Reusability**: Configuration classes can be reused in different hosting scenarios
6. **Discoverability**: Related services are grouped together logically

## Configuration Settings

IGDB credentials are now stored in `appsettings.json`:

```json
{
  "IGDB": {
    "ClientId": "your-client-id",
    "AccessToken": "your-access-token"
  }
}
```

## Adding New Services

To add new services:

1. Add them to the appropriate configuration class
2. If no suitable class exists, create a new one following the established pattern
3. Register the new configuration class in `ServiceConfiguration.AddApplicationServices()`

## Extension Method Pattern

All configuration classes follow the extension method pattern:
- Methods extend `IServiceCollection`
- Return `IServiceCollection` for method chaining
- Use meaningful names that describe their purpose