# FUTA Medical Booking System - Backend API

A comprehensive medical appointment booking system for Nigerian universities, specifically designed for the Federal University of Technology, Akure (FUTA).

## Architecture

This project follows **Clean Architecture** principles with **CQRS** pattern implementation:

```
├── FutaMedical.Domain          # Core business entities and domain events
├── FutaMedical.Application     # Business logic, CQRS handlers, DTOs
├── FutaMedical.Infrastructure  # EF Core, PostgreSQL, Identity services
└── FutaMedical.API             # Web API controllers and middleware
```

## Technology Stack

- **.NET 10.0** - Latest .NET framework
- **PostgreSQL 14+** - Relational database
- **Entity Framework Core 10.0.3** - ORM with Code-First migrations
- **MediatR 14.0** - CQRS and mediator pattern implementation
- **FluentValidation 12.1** - Input validation
- **JWT Authentication** - Secure token-based auth
- **BCrypt.Net** - Password hashing
- **Dapper** - High-performance database queries

## Features

### User Roles
- **Admin** - System administrators with full access
- **Doctor** - Medical practitioners managing appointments
- **Student** - FUTA students (patients) booking appointments

### Core Functionality

#### Implemented
- [x] User authentication (JWT with refresh tokens)
- [x] Student registration with comprehensive health information
- [x] Role-based authorization
- [x] Department management
- [x] Student profile management
- [x] Appointment creation with notifications
- [x] Database seeding with default users
- [x] Exception handling middleware
- [x] CORS configuration for frontend
- [x] Swagger/OpenAPI documentation

#### Planned
- [ ] Doctor appointment management (accept/reject/complete)
- [ ] Medical records with vital signs (JSONB storage)
- [ ] Prescription management
- [ ] Emergency request system
- [ ] Review and rating system
- [ ] Doctor availability scheduling
- [ ] Admin dashboard with statistics
- [ ] Notification system (real-time)
- [ ] Audit logging
- [ ] Email notifications

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 14 or higher
- Visual Studio 2022 / VS Code / Rider

### Database Setup

1. Install PostgreSQL and create a database:
```sql
CREATE DATABASE FutaMedicalDb;
```

2. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FutaMedicalDb;Username=postgres;Password=your_password"
  }
}
```

### Running the Application

1. Clone the repository
2. Navigate to the Backend folder:
```bash
cd Backend
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Create and apply migrations (when ready):
```bash
dotnet ef migrations add InitialCreate --project FutaMedical.Infrastructure --startup-project FutaMedical.API
dotnet ef database update --project FutaMedical.Infrastructure --startup-project FutaMedical.API
```

5. Run the API:
```bash
cd FutaMedical.API
dotnet run
```

The API will be available at: `https://localhost:5001` or `http://localhost:5000`

## API Documentation

### Default Seeded Accounts

#### Admin Account
- **Email**: admin@futa.edu.ng
- **Password**: Admin123!

#### Doctor Account
- **Email**: doctor@futa.edu.ng
- **Password**: Doctor123!
- **Department**: General Medicine
- **License**: MD123456

#### Student Account
- **Email**: student@futa.edu.ng
- **Password**: Student123!
- **Matric Number**: CSC/2020/001

### API Endpoints

#### Authentication
- `POST /api/auth/register` - Student registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh-token` - Refresh JWT token

#### Departments
- `GET /api/departments` - Get all departments (Public)

#### Students
- `GET /api/students/profile` - Get student profile (Student role)

#### Appointments
- `POST /api/appointments` - Create appointment (Student role)

For full API documentation, visit `/swagger` after running the application.

## Security

- **JWT Token Expiry**: 24 hours
- **Refresh Token Expiry**: 7 days
- **Password Requirements**:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 number
  - At least 1 special character
- **BCrypt** cost factor: 10-12

## Database Schema

### Key Tables
- **Users** - Base user information
- **Roles** - Admin, Doctor, Student
- **UserRoles** - Many-to-many relationship
- **Students** - Student-specific data with health information
- **Doctors** - Doctor-specific data with specializations
- **Appointments** - Appointment bookings
- **MedicalRecords** - Patient medical history with VitalSigns (JSONB)
- **Prescriptions** - Medication prescriptions
- **EmergencyRequests** - Emergency medical requests
- **Reviews** - Doctor reviews and ratings
- **Notifications** - In-app notifications
- **AuditLogs** - System audit trail

## Design Patterns

- **Clean Architecture** - Separation of concerns
- **CQRS** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction
- **Mediator Pattern** - Decoupled request handling
- **Domain Events** - Event-driven architecture for notifications
- **DRY (Don't Repeat Yourself)** - Code reusability
- **SOLID Principles** - Maintainable and scalable code

## Project Structure

```
FutaMedical.Domain/
├── Common/
│   └── BaseEntity.cs                    # Base entity with domain events
├── Entities/
│   ├── User.cs, Role.cs, Student.cs    # Core entities
│   ├── Doctor.cs, Department.cs        # Medical staff entities
│   ├── Appointment.cs                  # Appointment entity
│   ├── MedicalRecord.cs                # Medical records
│   └── UtilityEntities.cs              # Notifications, Reviews, etc.
├── ValueObjects/
│   └── VitalSigns.cs                   # Vital signs value object (JSONB)
└── Events/                             # Domain events (future)

FutaMedical.Application/
├── Common/
│   ├── Behaviours/
│   │   └── ValidationBehaviour.cs      # FluentValidation pipeline
│   └── Interfaces/
│       ├── IApplicationDbContext.cs    # DbContext abstraction
│       └── IIdentityService.cs         # Identity service abstraction
├── Features/
│   ├── Auth/
│   │   ├── Commands/                   # Login, Register, RefreshToken
│   │   └── DTOs/                       # Auth DTOs
│   ├── Departments/
│   │   └── Queries/                    # Get departments
│   ├── Students/
│   │   └── Queries/                    # Student profile
│   └── Appointments/
│       └── Commands/                   # Create appointment
└── DependencyInjection.cs

FutaMedical.Infrastructure/
├── Identity/
│   ├── JwtService.cs                   # JWT token generation
│   └── IdentityService.cs              # Authentication logic
├── Persistence/
│   ├── ApplicationDbContext.cs         # EF Core DbContext
│   ├── ApplicationDbContextSeed.cs     # Database seeding
│   └── Configurations/                 # Entity configurations
└── DependencyInjection.cs

FutaMedical.API/
├── Controllers/
│   ├── AuthController.cs               # Authentication endpoints
│   ├── DepartmentsController.cs        # Department endpoints
│   ├── StudentsController.cs           # Student endpoints
│   └── AppointmentsController.cs       # Appointment endpoints
├── Common/
│   └── ApiResponse.cs                  # Standardized API responses
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  # Global error handling
└── Program.cs                          # Application entry point
```

## Testing

Run tests using:
```bash
dotnet test
```

## Frontend Integration

This backend is designed to work with a Next.js 14.0.4 frontend application.

**Frontend Base URL**: `http://localhost:3000`  
**API Base URL**: `http://localhost:5000/api`

CORS is configured to allow requests from the frontend.

## NuGet Packages

### Application Layer
- MediatR
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

### Infrastructure Layer
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.EntityFrameworkCore.Design
- BCrypt.Net-Next
- Dapper
- System.IdentityModel.Tokens.Jwt

### API Layer
- Microsoft.AspNetCore.Authentication.JwtBearer

## Contributing

This is a university project for FUTA. Contributions should align with the project requirements.

## License

This project is for educational purposes at the Federal University of Technology, Akure.

## Team

- **Frontend Developer**: [Your Name]
- **Backend Developer**: AI Implementation
- **Project Supervisor**: [Supervisor Name]
- **University**: Federal University of Technology, Akure (FUTA)

## Support

For issues or questions, contact the development team.

---

**Document Version**: 1.0  
**Last Updated**: February 19, 2026  
**Status**: Core Implementation Complete, Advanced Features In Progress
