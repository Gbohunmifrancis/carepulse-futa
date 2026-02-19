# FUTA Medical Booking System - API Documentation

**Base URL:** `https://futa-medical-7ac7576e354e.herokuapp.com`  
**Swagger UI:** `https://futa-medical-7ac7576e354e.herokuapp.com/swagger`

---

## Table of Contents

1. [Authentication Endpoints](#authentication-endpoints)
2. [Student Endpoints](#student-endpoints)
3. [Admin Endpoints](#admin-endpoints)
4. [Department Endpoints](#department-endpoints)
5. [Appointment Endpoints](#appointment-endpoints)
6. [Error Responses](#error-responses)

---

## Authentication Endpoints

### 1. Login

**Endpoint:** `POST /api/Auth/login`  
**Authorization:** None  
**Description:** Authenticate user and receive JWT tokens

#### Request Body
```json
{
  "email": "francisgbohunmi@gmail.com",
  "password": "Admin@123"
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "francisgbohunmi@gmail.com",
    "firstName": "Francis",
    "lastName": "Gbohunmi",
    "role": "Admin",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "550e8400-e29b-41d4-a716-446655440001",
    "tokenExpiration": "2026-02-20T22:30:00Z"
  },
  "errors": []
}
```

#### Error Response (400 Bad Request)
```json
{
  "success": false,
  "message": "Login failed",
  "data": null,
  "errors": [
    "Invalid email or password"
  ]
}
```

---

### 2. Register Student

**Endpoint:** `POST /api/Auth/register-student`  
**Authorization:** None  
**Description:** Register a new student account

#### Request Body
```json
{
  "email": "john.doe@student.futa.edu.ng",
  "password": "SecurePass@123",
  "confirmPassword": "SecurePass@123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+2348012345678",
  "dateOfBirth": "2002-05-15T00:00:00Z",
  "gender": "Male",
  "address": "123 Campus Road, Akure, Ondo State",
  "matricNumber": "MED/2020/001",
  "faculty": "College of Health Sciences",
  "department": "Medicine and Surgery",
  "yearOfStudy": 3,
  "bloodGroup": "O+",
  "genotype": "AA",
  "allergies": ["Penicillin", "Peanuts"],
  "emergencyContacts": [
    {
      "name": "Jane Doe",
      "relationship": "Mother",
      "phoneNumber": "+2348087654321"
    }
  ]
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Student registration successful",
  "data": {
    "userId": "660e8400-e29b-41d4-a716-446655440002",
    "email": "john.doe@student.futa.edu.ng",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Student",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "660e8400-e29b-41d4-a716-446655440003",
    "tokenExpiration": "2026-02-20T22:30:00Z"
  },
  "errors": []
}
```

#### Error Response (400 Bad Request)
```json
{
  "success": false,
  "message": "Registration failed",
  "data": null,
  "errors": [
    "Email is already registered",
    "Password must contain at least one uppercase letter",
    "Matric number already exists"
  ]
}
```

---

### 3. Refresh Token

**Endpoint:** `POST /api/Auth/refresh-token`  
**Authorization:** None  
**Description:** Get new access token using refresh token

#### Request Body
```json
{
  "refreshToken": "660e8400-e29b-41d4-a716-446655440003"
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "userId": "660e8400-e29b-41d4-a716-446655440002",
    "email": "john.doe@student.futa.edu.ng",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Student",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "770e8400-e29b-41d4-a716-446655440004",
    "tokenExpiration": "2026-02-20T23:00:00Z"
  },
  "errors": []
}
```

---

## Student Endpoints

### 1. Get Student Profile

**Endpoint:** `GET /api/Students/profile`  
**Authorization:** `Bearer {token}` (Student role required)  
**Description:** Get authenticated student's profile

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Student profile retrieved successfully",
  "data": {
    "id": "770e8400-e29b-41d4-a716-446655440005",
    "userId": "660e8400-e29b-41d4-a716-446655440002",
    "email": "john.doe@student.futa.edu.ng",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+2348012345678",
    "dateOfBirth": "2002-05-15T00:00:00Z",
    "gender": "Male",
    "address": "123 Campus Road, Akure, Ondo State",
    "matricNumber": "MED/2020/001",
    "faculty": "College of Health Sciences",
    "department": "Medicine and Surgery",
    "yearOfStudy": 3,
    "bloodGroup": "O+",
    "genotype": "AA",
    "allergies": ["Penicillin", "Peanuts"],
    "emergencyContacts": [
      {
        "name": "Jane Doe",
        "relationship": "Mother",
        "phoneNumber": "+2348087654321"
      }
    ],
    "isActive": true,
    "createdAt": "2026-02-19T10:30:00Z"
  },
  "errors": []
}
```

---

### 2. Update Student Profile

**Endpoint:** `PATCH /api/Students/profile`  
**Authorization:** `Bearer {token}` (Student role required)  
**Description:** Update student's profile (only mutable fields)

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Request Body
```json
{
  "phoneNumber": "+2348098765432",
  "address": "456 New Street, Akure, Ondo State",
  "faculty": "College of Health Sciences",
  "department": "Nursing Science",
  "yearOfStudy": 4,
  "bloodGroup": "O+",
  "genotype": "AA",
  "allergies": ["Penicillin"],
  "emergencyContacts": [
    {
      "name": "Jane Doe",
      "relationship": "Mother",
      "phoneNumber": "+2348087654321"
    },
    {
      "name": "John Doe Sr.",
      "relationship": "Father",
      "phoneNumber": "+2348099999999"
    }
  ]
}
```

**Note:** Immutable fields (cannot be updated):
- firstName
- lastName
- matricNumber
- dateOfBirth
- gender

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": true,
  "errors": []
}
```

#### Error Response (400 Bad Request)
```json
{
  "success": false,
  "message": "Update failed",
  "data": false,
  "errors": [
    "Invalid phone number format",
    "Year of study must be between 1 and 7"
  ]
}
```

---

## Admin Endpoints

### 1. Get All Students

**Endpoint:** `GET /api/Admin/students`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Get list of all students

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Students retrieved successfully",
  "data": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440005",
      "userId": "660e8400-e29b-41d4-a716-446655440002",
      "email": "john.doe@student.futa.edu.ng",
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": "+2348012345678",
      "matricNumber": "MED/2020/001",
      "faculty": "College of Health Sciences",
      "department": "Medicine and Surgery",
      "yearOfStudy": 3,
      "isActive": true,
      "createdAt": "2026-02-19T10:30:00Z"
    }
  ],
  "errors": []
}
```

---

### 2. Get All Doctors

**Endpoint:** `GET /api/Admin/doctors`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Get list of all doctors

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Doctors retrieved successfully",
  "data": [
    {
      "id": "880e8400-e29b-41d4-a716-446655440006",
      "userId": "990e8400-e29b-41d4-a716-446655440007",
      "email": "dr.smith@futa.edu.ng",
      "firstName": "James",
      "lastName": "Smith",
      "phoneNumber": "+2348011111111",
      "specialization": "General Medicine",
      "licenseNumber": "MDC/2015/12345",
      "departmentId": "aa0e8400-e29b-41d4-a716-446655440008",
      "department": "General Medicine",
      "isActive": true,
      "createdAt": "2026-01-15T08:00:00Z"
    }
  ],
  "errors": []
}
```

---

### 3. Create Doctor

**Endpoint:** `POST /api/Admin/doctors`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Create a new doctor account

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Request Body
```json
{
  "email": "dr.jones@futa.edu.ng",
  "password": "Doctor@123",
  "firstName": "Sarah",
  "lastName": "Jones",
  "phoneNumber": "+2348022222222",
  "specialization": "Cardiology",
  "licenseNumber": "MDC/2018/67890",
  "departmentId": "aa0e8400-e29b-41d4-a716-446655440008"
}
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Doctor created successfully",
  "data": {
    "success": true,
    "message": "Doctor created successfully",
    "doctorId": "bb0e8400-e29b-41d4-a716-446655440009"
  },
  "errors": []
}
```

#### Error Response (400 Bad Request)
```json
{
  "success": false,
  "message": "Failed to create doctor",
  "data": {
    "success": false,
    "message": "Email already exists",
    "doctorId": null
  },
  "errors": [
    "Email is already registered"
  ]
}
```

---

### 4. Activate Doctor

**Endpoint:** `POST /api/Admin/doctors/{doctorId}/activate`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Activate a doctor's account

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Doctor activated successfully",
  "data": true,
  "errors": []
}
```

---

### 5. Deactivate Doctor

**Endpoint:** `POST /api/Admin/doctors/{doctorId}/deactivate`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Deactivate a doctor's account

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Doctor deactivated successfully",
  "data": true,
  "errors": []
}
```

---

### 6. Activate Student

**Endpoint:** `POST /api/Admin/students/{studentId}/activate`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Activate a student's account

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Student activated successfully",
  "data": true,
  "errors": []
}
```

---

### 7. Deactivate Student

**Endpoint:** `POST /api/Admin/students/{studentId}/deactivate`  
**Authorization:** `Bearer {token}` (Admin role required)  
**Description:** Deactivate a student's account

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Student deactivated successfully",
  "data": true,
  "errors": []
}
```

---

## Department Endpoints

### 1. Get All Departments

**Endpoint:** `GET /api/Departments`  
**Authorization:** None  
**Description:** Get list of all medical departments

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Departments retrieved successfully",
  "data": [
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440008",
      "name": "General Medicine",
      "description": "General medical consultation and treatment",
      "createdAt": "2026-01-01T00:00:00Z"
    },
    {
      "id": "cc0e8400-e29b-41d4-a716-446655440010",
      "name": "Cardiology",
      "description": "Heart and cardiovascular system care",
      "createdAt": "2026-01-01T00:00:00Z"
    },
    {
      "id": "dd0e8400-e29b-41d4-a716-446655440011",
      "name": "Dermatology",
      "description": "Skin conditions and treatments",
      "createdAt": "2026-01-01T00:00:00Z"
    },
    {
      "id": "ee0e8400-e29b-41d4-a716-446655440012",
      "name": "Orthopedics",
      "description": "Bone, joint and muscle care",
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ],
  "errors": []
}
```

---

## Appointment Endpoints

### 1. Create Appointment

**Endpoint:** `POST /api/Appointments`  
**Authorization:** `Bearer {token}` (Student role required)  
**Description:** Book a new medical appointment

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Request Body
```json
{
  "doctorId": "880e8400-e29b-41d4-a716-446655440006",
  "departmentId": "aa0e8400-e29b-41d4-a716-446655440008",
  "appointmentDate": "2026-02-25T10:00:00Z",
  "reasonForVisit": "General checkup and consultation for recurring headaches",
  "symptoms": "Frequent headaches, dizziness, fatigue",
  "vitalSigns": {
    "temperature": 36.8,
    "bloodPressure": "120/80",
    "heartRate": 72,
    "respiratoryRate": 16,
    "weight": 68.5,
    "height": 175
  }
}
```

#### Success Response (201 Created)
```json
{
  "success": true,
  "message": "Appointment created successfully",
  "data": {
    "id": "ff0e8400-e29b-41d4-a716-446655440013",
    "studentId": "770e8400-e29b-41d4-a716-446655440005",
    "studentName": "John Doe",
    "doctorId": "880e8400-e29b-41d4-a716-446655440006",
    "doctorName": "Dr. James Smith",
    "departmentId": "aa0e8400-e29b-41d4-a716-446655440008",
    "departmentName": "General Medicine",
    "appointmentDate": "2026-02-25T10:00:00Z",
    "status": "Pending",
    "reasonForVisit": "General checkup and consultation for recurring headaches",
    "symptoms": "Frequent headaches, dizziness, fatigue",
    "vitalSigns": {
      "temperature": 36.8,
      "bloodPressure": "120/80",
      "heartRate": 72,
      "respiratoryRate": 16,
      "weight": 68.5,
      "height": 175
    },
    "createdAt": "2026-02-19T22:30:00Z"
  },
  "errors": []
}
```

---

### 2. Get Student Appointments

**Endpoint:** `GET /api/Appointments/student`  
**Authorization:** `Bearer {token}` (Student role required)  
**Description:** Get all appointments for the authenticated student

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Appointments retrieved successfully",
  "data": [
    {
      "id": "ff0e8400-e29b-41d4-a716-446655440013",
      "studentId": "770e8400-e29b-41d4-a716-446655440005",
      "studentName": "John Doe",
      "doctorId": "880e8400-e29b-41d4-a716-446655440006",
      "doctorName": "Dr. James Smith",
      "departmentId": "aa0e8400-e29b-41d4-a716-446655440008",
      "departmentName": "General Medicine",
      "appointmentDate": "2026-02-25T10:00:00Z",
      "status": "Pending",
      "reasonForVisit": "General checkup and consultation for recurring headaches",
      "symptoms": "Frequent headaches, dizziness, fatigue",
      "vitalSigns": {
        "temperature": 36.8,
        "bloodPressure": "120/80",
        "heartRate": 72,
        "respiratoryRate": 16,
        "weight": 68.5,
        "height": 175
      },
      "createdAt": "2026-02-19T22:30:00Z"
    }
  ],
  "errors": []
}
```

---

### 3. Get Doctor Appointments

**Endpoint:** `GET /api/Appointments/doctor`  
**Authorization:** `Bearer {token}` (Doctor role required)  
**Description:** Get all appointments for the authenticated doctor

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Appointments retrieved successfully",
  "data": [
    {
      "id": "ff0e8400-e29b-41d4-a716-446655440013",
      "studentId": "770e8400-e29b-41d4-a716-446655440005",
      "studentName": "John Doe",
      "studentMatricNumber": "MED/2020/001",
      "doctorId": "880e8400-e29b-41d4-a716-446655440006",
      "doctorName": "Dr. James Smith",
      "departmentId": "aa0e8400-e29b-41d4-a716-446655440008",
      "departmentName": "General Medicine",
      "appointmentDate": "2026-02-25T10:00:00Z",
      "status": "Pending",
      "reasonForVisit": "General checkup and consultation for recurring headaches",
      "symptoms": "Frequent headaches, dizziness, fatigue",
      "vitalSigns": {
        "temperature": 36.8,
        "bloodPressure": "120/80",
        "heartRate": 72,
        "respiratoryRate": 16,
        "weight": 68.5,
        "height": 175
      },
      "createdAt": "2026-02-19T22:30:00Z"
    }
  ],
  "errors": []
}
```

---

### 4. Update Appointment Status

**Endpoint:** `PUT /api/Appointments/{appointmentId}/status`  
**Authorization:** `Bearer {token}` (Doctor or Admin role required)  
**Description:** Update appointment status

#### Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Request Body
```json
{
  "status": "Confirmed",
  "notes": "Appointment confirmed for 10:00 AM"
}
```

**Valid Status Values:**
- `Pending`
- `Confirmed`
- `Completed`
- `Cancelled`

#### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Appointment status updated successfully",
  "data": true,
  "errors": []
}
```

---

## Error Responses

### Common Error Response Format
```json
{
  "success": false,
  "message": "Operation failed",
  "data": null,
  "errors": [
    "Specific error message 1",
    "Specific error message 2"
  ]
}
```

### HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 400 | Bad Request - Invalid request data |
| 401 | Unauthorized - Missing or invalid token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error |

---

## Authentication

All protected endpoints require a JWT token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiration
- Access Token: 24 hours (1440 minutes)
- Refresh Token: 7 days

After the access token expires, use the refresh token endpoint to get a new access token without requiring the user to log in again.

---

## Data Types Reference

### Blood Groups
- `A+`, `A-`, `B+`, `B-`, `AB+`, `AB-`, `O+`, `O-`

### Genotypes
- `AA`, `AS`, `SS`, `AC`, `SC`

### Gender
- `Male`, `Female`, `Other`

### Appointment Status
- `Pending` - Appointment requested, awaiting confirmation
- `Confirmed` - Appointment confirmed by doctor/admin
- `Completed` - Appointment has been completed
- `Cancelled` - Appointment has been cancelled

### User Roles
- `Admin` - Full system access
- `Doctor` - Can view and manage appointments
- `Student` - Can book appointments and manage profile

---

## Test Credentials

### Admin
- **Email:** francisgbohunmi@gmail.com
- **Password:** Admin@123

### Doctor
- **Email:** doctor@futa.edu.ng
- **Password:** Doctor@123

### Student
- **Email:** student@futa.edu.ng
- **Password:** Student@123

---

## Notes for Frontend Developers

1. **Always include the Authorization header** for protected endpoints
2. **Handle token expiration** by implementing automatic refresh token logic
3. **Validate user input** on the frontend before sending requests
4. **Display appropriate error messages** from the `errors` array in responses
5. **Store tokens securely** (avoid localStorage for sensitive data, consider httpOnly cookies)
6. **Implement loading states** while waiting for API responses
7. **Handle network errors gracefully** with retry logic or user feedback
8. **Date/Time format**: All dates are in ISO 8601 format (UTC)
9. **Phone numbers**: Should include country code (e.g., +234)
10. **CORS**: The API is configured to accept requests from the Heroku domain and localhost:3000

---

## Support

For questions or issues, contact the backend team or refer to the Swagger documentation at:
`https://futa-medical-7ac7576e354e.herokuapp.com/swagger`
