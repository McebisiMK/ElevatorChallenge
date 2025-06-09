[![Continuous Integration and deployment](https://github.com/McebisiMK/ElevatorChallenge/actions/workflows/dotnet.yml/badge.svg)](https://github.com/McebisiMK/ElevatorChallenge/actions/workflows/dotnet.yml)
# ElevatorChallenge
Simulates the movement of elevators within a large building, with the aim of optimizing passenger transportation efficiently.
- Intelligent assignment of pickup requests to the most suitable elevator
- Directional awareness (Up/Down) for efficient routing
- Passenger limits, waiting queues, and dynamic goal reordering
- Console-based interaction and extensible service registration

## Architecture & Projects
### [ElevatorChallenge.Console](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Console)
> **Purpose**: A console-based user interface for simulating elevator behavior. 
>
**Includes:**
- Main [Program.cs](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Console/Program.cs)
- Console output/input handling
- Bootstrapping via [Dependency Injection](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Console/DependencyInjection.cs)

### [ElevatorChallenge.Application](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Application)
> **Purpose:** Core domain logic, scheduling, and request handling using the CQRS **(Command Query Responsibility Segregation)** pattern.
>
**Includes:**
- Interfaces like [IElevator](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Application/Common/Interfaces/IElevator.cs), [IElevatorService](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Application/Common/Interfaces/IElevatorService.cs)
- CQRS-based handlers for queries (e.g. [Get all standard elevators](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Application/Queries/Elevators/GetElevators/GetStandardElevatorsQuery.cs), [Get Nearest Elevator](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Application/Queries/Elevators/GetNearest/GetNearestAvailableElevatorQuery.cs))
- Scheduling logic (e.g., pickup resolution, direction switching)
- input validation like [Get Standard Elevator Query Validator](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Application/Queries/Elevators/GetElevators/GetStandardElevatorsQueryValidator.cs).
- [Dependency Injection](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Application/DependencyInjection.cs) setup (`RegisterApplication`)

### [ElevatorChallenge.Domain](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Domain)
> **Purpose:** Defines the core **enterprise logic**, including entities and enums. This layer is completely free of any dependencies.
>
**Includes:**
- [Domain Entities](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Domain/Entities)
- [Domain Enums](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Domain/Enums)

### [ElevatorChallenge.Infrastructure](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Infrastructure)
> **Purpose**: Provides **implementations and integrations** for interfaces defined in the application layer.  
>
**Includes**:  
- [Dependency Injection](https://github.com/McebisiMK/ElevatorChallenge/blob/main/ElevatorChallenge.Infrastructure/DependencyInjection.cs) setup (`RegisterInfrastructure`)  
- Service implementations supporting application logic 

### [ElevatorChallenge.Tests](https://github.com/McebisiMK/ElevatorChallenge/tree/main/ElevatorChallenge.Tests)
> **Purpose**: Unit test project validating domain and application logic.  
>
**Includes**:  
- Tests for core domain behaviors and command/query handlers  
- Use of FluentAssertions and mocking via NSubstitute  
- Scenario coverage for scheduling, capacity, and direction logic

### Technologies & Tools Used
> The project leverages modern .NET ecosystem tools and practices to ensure maintainability, testability, and clean separation of concerns:
>
- **.NET SDK 9.0** - for building and running the application
- **C# 12** - modern language features
- **Clean Architecture** - layered structure promoting separation of concerns
- **CQRS Pattern** - separates commands and queries for clarity and scalability
- **Dependency Injection (DI)** - via `Microsoft.Extensions.DependencyInjection`
- **FluentValidation** - for validating incoming requests and inputs
- **MediatR** - to handle request/response messaging in CQRS
- **FluentAssertions** - expressive assertions for unit testing
- **NSubstitute** - mocking dependencies in tests
- **NUnit** - test framework for verifying behavior
- **GitHub Actions** - CI pipeline for build and test automation
