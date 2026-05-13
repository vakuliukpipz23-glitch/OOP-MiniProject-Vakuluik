# Library Management System

A comprehensive OOP mini-project implementing a library management system with a layered architecture demonstrating SOLID principles, design patterns, and clean code practices.

## Project Overview

The Library Management System (LMS) is a console-based application that manages library operations including:
- Patron registration and management
- Book inventory management
- Book borrowing and returns
- Overdue fee tracking

This project serves as an educational implementation for Iteration 1 of the OOP Mini-Project Lab 34.

## Architecture

The system follows a **Layered Architecture** pattern with clear separation of concerns:

```
src/
├── LibraryManagementSystem.Domain/          # Business entities & rules
│   ├── Book.cs                              # Book entity with validation
│   ├── Copy.cs                              # Physical copy of a book
│   ├── Patron.cs                            # Library patron
│   ├── BorrowRecord.cs                      # Borrow transaction record
│   ├── OverduePolicy.cs                     # Business rules for overdue fees
│   ├── CopyStatus.cs                        # Enumeration for copy status
│   └── Repositories/                        # Repository interfaces
│       └── IPatronRepository, IBookRepository, IBorrowRepository
│
├── LibraryManagementSystem.Application/     # Business logic & use cases
│   ├── PatronService.cs                     # Patron management logic
│   ├── BookService.cs                       # Book management logic
│   └── BorrowService.cs                     # Borrowing orchestration
│
├── LibraryManagementSystem.Infrastructure/  # Data access implementations
│   ├── InMemoryPatronRepository.cs          # In-memory patron storage
│   ├── InMemoryBookRepository.cs            # In-memory book storage
│   └── InMemoryBorrowRepository.cs          # In-memory borrow record storage
│
└── LibraryManagementSystem.Console/         # User interface
    └── Program.cs                           # Main console application

tests/
└── LibraryManagementSystem.Tests/           # Unit tests
    ├── DomainModelTests.cs                  # Domain entity tests
    └── ServiceTests.cs                      # Service layer tests
```

## Key Features

### Iteration 1 (Lab 34)

**Vertical Slice: Register Patron → Borrow Book → Return Book**

1. **Domain Layer**
   - 5+ domain entities (Book, Copy, Patron, BorrowRecord, OverduePolicy)
   - Input validation in constructors
   - Business rule enforcement
   - Encapsulated properties

2. **Application Layer**
   - Three main services (PatronService, BookService, BorrowService)
   - Dependency injection through constructors
   - Orchestration of complex operations

3. **Infrastructure Layer**
   - Repository pattern implementation
   - In-memory data persistence
   - Prepared for SQL implementation in Lab 35

4. **Console UI**
   - Interactive menu-driven interface
   - Complete scenario walkthroughs
   - Error handling and user feedback

5. **Testing**
   - 15+ unit tests covering domain logic
   - Service layer integration tests
   - Repository pattern validation

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Git

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/OOP-MiniProject-Vakuluik.git
cd OOP-MiniProject-Vakuluik

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Running the Application

```bash
# Run the console application
dotnet run --project src/LibraryManagementSystem.Console
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## Usage Example

### Main Workflow

1. **Start Application**: Run the console app
2. **Register Patron**: Use menu option 1
3. **Register Book**: Use menu option 3 (sample books pre-loaded)
4. **Borrow Book**: Use menu option 5
   - Enter patron ID
   - Enter book ISBN
   - System creates borrow record with 30-day due date
5. **Return Book**: Use menu option 6
   - Enter borrow ID
   - System calculates overdue fees if applicable
   - Copy is marked as available

## Design Patterns Implemented

1. **Repository Pattern** - Abstraction of data access logic
2. **Service Pattern** - Encapsulation of business logic
3. **Dependency Injection** - Loose coupling between layers
4. **Entity & Value Object** - Clear distinction between mutable and immutable data
5. **Policy Pattern** - OverduePolicy for business rules
6. **Factory** - ID generation patterns in services

## SOLID Principles Applied

- **S** (Single Responsibility): Each class has one reason to change
- **O** (Open/Closed): Open for extension (new services), closed for modification
- **L** (Liskov Substitution): Repository implementations are interchangeable
- **I** (Interface Segregation): Separate repository interfaces for different concerns
- **D** (Dependency Inversion): Depends on abstractions, not concrete implementations

## Code Quality

- **Clean Code**: Clear naming, small methods, no deep nesting
- **Encapsulation**: Private fields with property access where needed
- **Validation**: Input validation in constructors and services
- **Error Handling**: Explicit exception throwing with descriptive messages
- **Documentation**: XML comments for public APIs

## Current Limitations (Iteration 1 Scope)

- **In-memory storage only** (will add SQL in Lab 35)
- **No authentication/authorization** (future feature)
- **No web API** (planned for future)
- **Basic console UI** (web UI planned for Lab 37)
- **No advanced queries** (filtering, sorting in Lab 35)

## Future Enhancements (Lab 35-37)

- SQL Server persistence layer
- Advanced search and filtering
- Book reservations system
- Email notifications
- REST API implementation
- Web UI (ASP.NET Core)
- Mobile app
- Analytics dashboard

## Project Structure Requirements Met

- [x] `docs/vision.md` - Problem statement and requirements
- [x] `docs/backlog.md` - Iteration planning
- [x] `docs/class-diagram.md` - UML class diagram with Mermaid
- [x] `docs/sequence-diagram.md` - Sequence diagram for main scenario
- [x] Solution with 5 projects (Domain, Application, Infrastructure, Console, Tests)
- [x] Domain layer with 5+ entities and business rules
- [x] Repository pattern with in-memory implementations
- [x] Working vertical slice (register → borrow → return)
- [x] 15+ unit tests
- [x] GitHub Actions CI/CD workflow
- [x] README with setup instructions
- [x] `.gitignore` for .NET projects

## Testing

The project includes comprehensive unit tests:

```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~DomainModelTests"

# Run specific test method
dotnet test --filter "Name~Book_Constructor_WithValidData_CreatesBook"
```

## CI/CD Pipeline

The GitHub Actions workflow automatically:
1. Restores NuGet packages
2. Builds the solution
3. Runs all unit tests on every push to main or develop branches

## Contributing

This is an educational project for learning purposes. See `docs/iteration-1.md` for current status and known limitations.

## License

This project is created for educational purposes in the OOP Mini-Project course.

## Contact & Support

For questions or issues, refer to the lab documentation or course materials.

---

**Status**: Lab 34 (Iteration 1) - Foundation Complete
**Next**: Lab 35 (Iteration 2) - Data Persistence & Advanced Features
