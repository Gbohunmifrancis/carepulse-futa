# Doctor Onboarding Workflow Documentation

## Overview
This document describes the complete doctor onboarding and verification workflow implemented in the FutaMedical system.

## Workflow Summary

```
Admin Creates Doctor → Email Invitation → Set Password → Complete Onboarding → Admin Review → Verification
```

## Key Features

### 1. Account Creation (Admin)
- **Endpoint**: `POST /api/admin/doctors`
- **Who**: Admin only
- **Required**: Email, Phone Number (optional)
- **Result**: 
  - Doctor account created with `IsVerified = false`
  - `IsActive = false` until password is set
  - Password setup token generated (valid for 7 days)
  - Setup token returned in response (to be sent via email)

**Request Example**:
```json
{
  "email": "doctor@example.com",
  "phoneNumber": "+2348012345678"
}
```

**Response Example**:
```json
{
  "success": true,
  "message": "Doctor invitation sent successfully",
  "data": {
    "doctorId": "guid-here",
    "setupToken": "secure-token-here"
  }
}
```

### 2. Password Setup (Doctor)
- **Endpoint**: `POST /api/auth/set-password`
- **Who**: New doctor (no authentication required)
- **Required**: Setup token, Password, First Name, Last Name
- **Result**: 
  - Password set and account activated (`IsActive = true`)
  - Doctor can now login
  - Still `IsVerified = false`

**Request Example**:
```json
{
  "token": "secure-token-here",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 3. Complete Onboarding (Doctor)
- **Endpoint**: `POST /api/doctors/onboarding/complete`
- **Who**: Authenticated doctor (requires login)
- **Authorization**: `[Authorize(Roles = "Doctor")]`
- **Required**:
  - Department ID
  - Specialization
  - License Number (unique)
  - Qualifications (optional)
  - Years of Experience
  - Bio (optional)
  - Identification Document (URL/path)
  - Certificate Document (URL/path)
- **Result**:
  - Application status set to "Pending"
  - Notification sent to admin (TODO)
  - Doctor still `IsVerified = false`

**Request Example**:
```json
{
  "departmentId": "dept-guid-here",
  "specialization": "Cardiology",
  "licenseNumber": "MCN-12345",
  "qualifications": "MBBS, MD",
  "yearsOfExperience": 5,
  "bio": "Experienced cardiologist...",
  "identificationDocument": "https://storage.example.com/docs/id-123.pdf",
  "certificateDocument": "https://storage.example.com/docs/cert-123.pdf"
}
```

### 4. View Pending Applications (Admin)
- **Endpoint**: `GET /api/admin/doctors/pending`
- **Who**: Admin only
- **Returns**: List of all pending doctor applications with full details

**Response Example**:
```json
{
  "success": true,
  "data": [
    {
      "doctorId": "guid-here",
      "email": "doctor@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "specialization": "Cardiology",
      "licenseNumber": "MCN-12345",
      "identificationDocument": "https://...",
      "certificateDocument": "https://...",
      "applicationSubmittedAt": "2026-02-23T10:00:00Z"
    }
  ]
}
```

### 5. Review Application (Admin)
- **Endpoint**: `POST /api/admin/doctors/{id}/review`
- **Who**: Admin only
- **Required**: Approve (true/false), RejectionReason (if rejecting)
- **Result**:
  - If approved: `IsVerified = true`, `ApplicationStatus = "Approved"`
  - If rejected: `IsVerified = false`, `ApplicationStatus = "Rejected"`
  - Email notification sent to doctor (TODO)

**Approve Request Example**:
```json
{
  "approve": true
}
```

**Reject Request Example**:
```json
{
  "approve": false,
  "rejectionReason": "Invalid license number"
}
```

## Important Rules

### ✅ Verified Doctors
- Can accept appointments
- Appear in available doctors list
- Show in booking interface

### ❌ Unverified/Pending Doctors
- **CANNOT** be booked for appointments
- Appointment creation endpoint checks `IsVerified` status
- Returns error: "Doctor not found or not verified"

## Database Schema Changes

### User Entity
```csharp
public string? PasswordSetupToken { get; set; }
public DateTime? PasswordSetupTokenExpiry { get; set; }
```

### Doctor Entity
```csharp
public Guid? DepartmentId { get; set; }  // Now nullable
public string? Specialization { get; set; }  // Now nullable
public string? LicenseNumber { get; set; }  // Now nullable
public string? ApplicationStatus { get; set; }  // "Pending", "Approved", "Rejected"
public DateTime? ApplicationSubmittedAt { get; set; }
public DateTime? ApplicationReviewedAt { get; set; }
public string? Bio { get; set; }
public string? IdentificationDocument { get; set; }
public string? CertificateDocument { get; set; }
```

## Migration
- Migration name: `AddDoctorOnboardingWorkflow`
- Applied: Yes (local database)

## API Endpoints Summary

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `/api/admin/doctors` | POST | Admin | Create doctor invitation |
| `/api/auth/set-password` | POST | Public | Set password using token |
| `/api/doctors/onboarding/complete` | POST | Doctor | Submit onboarding details |
| `/api/admin/doctors/pending` | GET | Admin | Get pending applications |
| `/api/admin/doctors/{id}/review` | POST | Admin | Approve/reject application |
| `/api/admin/doctors` | GET | Admin | Get all doctors (with status) |

## Testing Workflow

1. **Admin creates doctor**:
   ```bash
   POST /api/admin/doctors
   Body: { "email": "test@example.com" }
   ```

2. **Doctor sets password**:
   ```bash
   POST /api/auth/set-password
   Body: { "token": "...", "password": "...", "firstName": "...", "lastName": "..." }
   ```

3. **Doctor logs in**:
   ```bash
   POST /api/auth/login
   Body: { "email": "test@example.com", "password": "..." }
   ```

4. **Doctor completes onboarding**:
   ```bash
   POST /api/doctors/onboarding/complete
   Headers: Authorization: Bearer <token>
   Body: { "departmentId": "...", "specialization": "...", ... }
   ```

5. **Admin views pending applications**:
   ```bash
   GET /api/admin/doctors/pending
   Headers: Authorization: Bearer <admin-token>
   ```

6. **Admin reviews application**:
   ```bash
   POST /api/admin/doctors/{id}/review
   Headers: Authorization: Bearer <admin-token>
   Body: { "approve": true }
   ```

## TODO Items

### Email Notifications
- [ ] Send invitation email with setup link when admin creates doctor
- [ ] Send welcome email after password setup
- [ ] Send notification to admin when doctor submits onboarding
- [ ] Send approval email to doctor
- [ ] Send rejection email with reason to doctor

### File Upload
- [ ] Implement file upload endpoint for documents
- [ ] Store documents securely (AWS S3, Azure Blob, etc.)
- [ ] Return document URLs for storage in database

### Frontend Integration
- [ ] Doctor invitation page for admins
- [ ] Password setup page (accessible via email link)
- [ ] Doctor onboarding form with file uploads
- [ ] Admin review dashboard showing pending applications
- [ ] Document viewer for admin review

## Security Considerations

1. **Token Expiry**: Password setup tokens expire after 7 days
2. **One-time Use**: Tokens are cleared after password is set
3. **Authorization**: Only authenticated doctors can complete onboarding
4. **Admin Only**: Only admins can create doctors and review applications
5. **License Uniqueness**: License numbers must be unique across all doctors
6. **Appointment Protection**: Unverified doctors cannot be booked

## Status Flow Diagram

```
Doctor Created
    ↓
IsVerified: false
IsActive: false
ApplicationStatus: null
    ↓
Password Set → IsActive: true
    ↓
Onboarding Submitted → ApplicationStatus: "Pending"
    ↓
Admin Review
    ↓
  ├─→ Approved → IsVerified: true, ApplicationStatus: "Approved"
  └─→ Rejected → IsVerified: false, ApplicationStatus: "Rejected"
```

## Notes

- Doctors can only submit onboarding once (checked by ApplicationStatus)
- Admins can view all doctors with their current status using `GET /api/admin/doctors`
- Pending applications are ordered by submission date (oldest first)
- Rejected doctors remain in the system but cannot accept appointments
- Admin can deactivate any doctor using `POST /api/admin/doctors/{id}/deactivate`
