# TicketingPlatform Backend

A modular, scalable ticketing platform backend built with ASP.NET Core (.NET 8+), designed for event organizers and attendees. This project follows best practices for maintainability, testability, and modular separation.

---

## 🏗️ Architecture Overview

- **Modular Monolith:**
  - Single API entry point (`TicketingPlatform.API`)
  - Four functional modules as class libraries:
    - `Modules.UserService`
    - `Modules.EventService`
    - `Modules.TicketService`
    - `Modules.PaymentService`
  - Shared kernel for cross-cutting concerns: `Shared.Kernel`
  - Each module exposes DI and endpoint extension methods for composition
- **Unit Test Projects:**
  - `Tests.UserService.Tests`, `Tests.EventService.Tests`, etc. (xUnit)

---

## 📁 Project Structure

```
/Modules.UserService
  /Controllers
  /Services
  /Repositories
  /Models
  /DTOs
/Modules.EventService
  ...
/Modules.TicketService
  ...
/Modules.PaymentService
  ...
/Shared.Kernel
  /Enums
  /Interfaces
  /BaseEntities
  /ResultTypes
/TicketingPlatform.API
/Tests.UserService.Tests
/Tests.EventService.Tests
/Tests.TicketService.Tests
/Tests.PaymentService.Tests
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK or later](https://dotnet.microsoft.com/download)
- (Optional) [Docker](https://www.docker.com/) for containerization

### Build the Solution
```sh
dotnet build TicketingPlatform.sln
```

### Run the API
```sh
dotnet run --project TicketingPlatform.API
```

The API will be available at `http://localhost:5000` (or as configured).

### Run Unit Tests
```sh
dotnet test TicketingPlatform.sln
```

---

## 🧩 Module Composition
- Each module exposes:
  - `ServiceCollectionExtensions.cs` for DI registration (e.g., `services.AddUserModule()`)
  - `EndpointMapper.cs` for endpoint registration (e.g., `app.MapUserEndpoints()`)
- Compose modules in `TicketingPlatform.API/Program.cs`:
  ```csharp
  builder.Services.AddUserModule();
  app.MapUserEndpoints();
  // ...repeat for other modules
  ```

---

## 🧪 Testing
- 100% unit test coverage is enforced for all modules.
- Test projects are located under `/Tests.*.Tests`.
- Use xUnit for all unit tests.

---

## 🛡️ Best Practices
- All methods must be documented with XML comments.
- All APIs are documented via Swagger (OpenAPI).
- Role-based access control and JWT authentication are enforced.
- Centralized logging and error handling are recommended.

---

## 🤝 Contributing
1. Fork the repo and create a feature branch.
2. Follow the modular structure and naming conventions.
3. Write unit tests for all new code.
4. Document all public methods and APIs.
5. Submit a pull request with a clear description.

---

## 📚 References
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [xUnit Documentation](https://xunit.net/docs/)
- [Swagger/OpenAPI](https://swagger.io/docs/)

---

## 📄 License
MIT 