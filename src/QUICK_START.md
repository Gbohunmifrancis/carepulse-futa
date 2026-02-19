# Quick Start Guide - FUTA Medical Backend

## Prerequisites
- .NET 10.0 SDK installed
- PostgreSQL 14+ installed and running
- Your favorite IDE (VS Code, Visual Studio, Rider)

## Setup Steps

### 1. Database Configuration
Create a PostgreSQL database:
```sql
CREATE DATABASE FutaMedicalDb;
```

Update connection string in `FutaMedical.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=FutaMedicalDb;Username=postgres;Password=YOUR_PASSWORD_HERE"
}
```

### 2. Run the Application
```bash
cd Backend/FutaMedical.API
dotnet run
```

The API will start at: **https://localhost:5001**

### 3. Access Swagger Documentation
Open your browser: **https://localhost:5001/swagger**

## Test the API

### 1. Login as Admin
```bash
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@futa.edu.ng",
  "password": "Admin123!"
}
```

### 2. Register a New Student
```bash
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "Student",
  "email": "test.student@futa.edu.ng",
  "phoneNumber": "+2348012345678",
  "password": "Test123!@#",
  "matricNumber": "CSC/2023/999",
  "dateOfBirth": "2002-01-15",
  "gender": "Male",
  "faculty": "Engineering",
  "department": "Computer Science",
  "yearOfStudy": 2,
  "bloodGroup": "O+",
  "genotype": "AA",
  "allergies": "None",
  "emergencyContactName": "Parent Name",
  "emergencyContactPhone": "+2348099999999"
}
```

### 3. Get Student Profile (With JWT Token)
```bash
GET https://localhost:5001/api/students/profile
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

### 4. Create an Appointment
```bash
POST https://localhost:5001/api/appointments
Authorization: Bearer STUDENT_JWT_TOKEN
Content-Type: application/json

{
  "doctorId": "USE_DOCTOR_ID_FROM_DATABASE",
  "appointmentDate": "2026-02-25T09:00:00Z",
  "startTime": "09:00",
  "reason": "Headache and fever"
}
```

## Default Test Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@futa.edu.ng | Admin123! |
| Doctor | doctor@futa.edu.ng | Doctor123! |
| Student | student@futa.edu.ng | Student123! |

## Common Commands

### Build the solution
```bash
cd Backend
dotnet build
```

### Run tests
```bash
dotnet test
```

### Create migration (when ready)
```bash
dotnet ef migrations add InitialCreate --project FutaMedical.Infrastructure --startup-project FutaMedical.API
```

### Apply migration
```bash
dotnet ef database update --project FutaMedical.Infrastructure --startup-project FutaMedical.API
```

## Project Structure
```
Backend/
├── FutaMedical.Domain/           # Entities, Value Objects
├── FutaMedical.Application/      # CQRS Handlers, DTOs
├── FutaMedical.Infrastructure/   # DbContext, Identity
└── FutaMedical.API/              # Controllers, Middleware
```

## Troubleshooting

### Build Errors
- Ensure .NET 10.0 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

### Database Connection Issues
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Ensure database exists

### JWT Token Issues
- Tokens expire after 24 hours
- Use refresh token endpoint to get a new token
- Ensure Authorization header format: `Bearer {token}`

## Next Development Steps

1. ✅ Authentication working
2. ✅ Student registration working
3. ✅ Appointment creation working
4. 🚧 Implement doctor appointment management
5. 🚧 Add medical records functionality
6. 🚧 Create admin dashboard
7. 🚧 Add notification system

## Getting Help

- Check the README.md for detailed documentation
- Review IMPLEMENTATION_SUMMARY.md for architecture details
- Swagger UI provides interactive API documentation

---

🚀 **Ready to start development!**
