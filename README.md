# Modular Monolith Template

A production-ready template for building .NET applications using the **Modular Monolith** architecture pattern with Clean Architecture principles.

## Overview

This template provides a solid foundation for building scalable, maintainable applications that can evolve from a monolith to microservices when needed. It combines the simplicity of a monolith with the modularity of microservices.

## Architecture

### Modular Monolith Pattern

Each module is self-contained with its own:
- **Domain Layer** - Business entities, value objects, and domain logic
- **Application Layer** - Use cases, services, and business rules
- **Infrastructure Layer** - Data persistence, external services integration
- **Presentation Layer** - API endpoints and DTOs

### Key Principles
- **Module Independence** - Modules are isolated and communicate through well-defined interfaces
- **Clean Architecture** - Clear separation of concerns with dependency inversion
- **Shared Kernel** - Common abstractions and contracts shared across modules

## Technologies

- **.NET 10.0**
- **Aspire** - Cloud-ready stack for building observable, production-ready distributed applications
- **Wolverine** - Message bus and mediator for in-process and distributed messaging
- **Entity Framework Core** - Data access and persistence
- **OpenAPI/Swagger** - API documentation

## Project Structure

```
├── VertoBank/                    # Main API host application
├── VertoBank.AppHost/            # Aspire orchestration host
├── VertoBank.ServiceDefaults/    # Shared service configuration (OpenTelemetry, Health Checks)
└── VertoBank.Modules/
    ├── SharedKernel/             # Common interfaces and abstractions
    └── Module/                   # Template module (copy this to create new modules)
        ├── Module/               # Module entry point and installer
        ├── Module.Domain/        # Domain entities and logic
        ├── Module.Application/   # Application services and use cases
        ├── Module.Infrastructure/# Data access and external services
        └── Module.Presentation/  # API endpoints
```

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Your preferred IDE (Visual Studio, VS Code, Rider)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/kezniklm/Modular-Monolith-Template
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run with Aspire (recommended)**
   ```bash
   dotnet run --project VertoBank.AppHost
   ```

4. **Or run standalone**
   ```bash
   dotnet run --project VertoBank
   ```

## Creating a New Module

1. Copy the `VertoBank.Modules/Module` folder
2. Rename all `Module` references to your new module name
3. Register the module in `VertoBank/DependencyInjection.cs`:
   ```csharp
   var modules = new List<IModule>
   {
       new ModuleInstaller(),
       new YourNewModuleInstaller() // Add your module here
   };
   ```
4. Add connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "YourNewModuleConnectionString": "..."
     }
   }
   ```

## Module Interface

Each module implements the `IModule` interface:

```csharp
public interface IModule
{
    void InstallDomain(IServiceCollection serviceCollection);
    void InstallApplication(IServiceCollection serviceCollection);
    void InstallInfrastructure(IServiceCollection serviceCollection, string connectionString);
    void InstallPresentation(IServiceCollection serviceCollection);
}
```

## Configuration

- `appsettings.json` - Application configuration
- `appsettings.Development.json` - Development-specific settings
- Connection strings follow the pattern: `{ModuleName}ConnectionString`

## License

View [LICENSE.txt](LICENSE.txt) for licensing information.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.