using System;
using System.Linq;
using System.Threading.Tasks;
using FutaMedical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BCryptLib = BCrypt.Net.BCrypt;

namespace FutaMedical.Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System Administrator", CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.NewGuid(), Name = "Doctor", Description = "Medical Practitioner", CreatedAt = DateTime.UtcNow },
                new Role { Id = Guid.NewGuid(), Name = "Student", Description = "Student/Patient", CreatedAt = DateTime.UtcNow }
            };
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync(default);
        }

        // Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            var departments = new[]
            {
                new Department { Id = Guid.NewGuid(), Name = "General Medicine", Description = "General medical consultations and treatment", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Dentistry", Description = "Dental care and oral health", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Gynecology", Description = "Women's health and reproductive care", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Pediatrics", Description = "Child healthcare and development", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Orthopedics", Description = "Bone, joint and muscle care", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            context.Departments.AddRange(departments);
            await context.SaveChangesAsync(default);
        }

        // Seed Default Admin
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
            var adminUserExists = await context.Users.FirstOrDefaultAsync(u => u.Email == "francisgbohunmi@gmail.com");
            
            if (adminUserExists == null)
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "francisgbohunmi@gmail.com",
                    PasswordHash = BCryptLib.HashPassword("Admin123!"),
                    FirstName = "System",
                    LastName = "Administrator",
                    PhoneNumber = "+2348000000000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync(default);

                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    AssignedAt = DateTime.UtcNow
                });

                context.Admins.Add(new Admin
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync(default);
            }
            else
            {
                // Ensure UserRole exists
                var hasRole = await context.UserRoles.AnyAsync(ur => ur.UserId == adminUserExists.Id && ur.RoleId == adminRole.Id);
                if (!hasRole)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUserExists.Id,
                        RoleId = adminRole.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync(default);
                }
            }
        }

        // Seed Sample Doctor (Optional for testing)
        var doctorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
        var generalMedicineDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "General Medicine");
        
        if (doctorRole != null && generalMedicineDept != null)
        {
            var doctorUserExists = await context.Users.FirstOrDefaultAsync(u => u.Email == "doctor@futa.edu.ng");
            
            if (doctorUserExists == null)
            {
                var doctorUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "doctor@futa.edu.ng",
                    PasswordHash = BCryptLib.HashPassword("Doctor123!"),
                    FirstName = "James",
                    LastName = "Smith",
                    PhoneNumber = "+2348111111111",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(doctorUser);
                await context.SaveChangesAsync(default);

                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = doctorUser.Id,
                    RoleId = doctorRole.Id,
                    AssignedAt = DateTime.UtcNow
                });

                context.Doctors.Add(new Doctor
                {
                    Id = Guid.NewGuid(),
                    UserId = doctorUser.Id,
                    DepartmentId = generalMedicineDept.Id,
                    Specialization = "General Practitioner",
                    LicenseNumber = "MD123456",
                    Qualifications = "MBBS, FMCP",
                    YearsOfExperience = 10,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync(default);
            }
            else
            {
                // Ensure UserRole exists
                var hasRole = await context.UserRoles.AnyAsync(ur => ur.UserId == doctorUserExists.Id && ur.RoleId == doctorRole.Id);
                if (!hasRole)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = doctorUserExists.Id,
                        RoleId = doctorRole.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync(default);
                }
            }
        }

        // Seed Sample Student (Optional for testing)
        var studentRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        
        if (studentRole != null)
        {
            var studentUserExists = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@futa.edu.ng");
            
            if (studentUserExists == null)
            {
                // Create new student user
                var studentUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "student@futa.edu.ng",
                    PasswordHash = BCryptLib.HashPassword("Student123!"),
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "+2348222222222",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(studentUser);
                await context.SaveChangesAsync(default);

                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = studentUser.Id,
                    RoleId = studentRole.Id,
                    AssignedAt = DateTime.UtcNow
                });

                context.Students.Add(new Student
                {
                    Id = Guid.NewGuid(),
                    UserId = studentUser.Id,
                    MatricNumber = "CSC/2020/001",
                    DateOfBirth = new DateTime(2000, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Male",
                    Address = "123 Student Ave, Akure",
                    Faculty = "Engineering",
                    Department = "Computer Science",
                    YearOfStudy = 3,
                    BloodGroup = "O+",
                    Genotype = "AA",
                    Allergies = "Penicillin",
                    EmergencyContactName = "Jane Doe",
                    EmergencyContactPhone = "+2348333333333",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync(default);
            }
            else
            {
                // Ensure UserRole exists for existing student user
                var hasRole = await context.UserRoles.AnyAsync(ur => ur.UserId == studentUserExists.Id && ur.RoleId == studentRole.Id);
                if (!hasRole)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = studentUserExists.Id,
                        RoleId = studentRole.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync(default);
                }
                
                // Ensure Student profile exists
                var hasStudentProfile = await context.Students.AnyAsync(s => s.UserId == studentUserExists.Id);
                if (!hasStudentProfile)
                {
                    context.Students.Add(new Student
                    {
                        Id = Guid.NewGuid(),
                        UserId = studentUserExists.Id,
                        MatricNumber = "CSC/2020/001",
                        DateOfBirth = new DateTime(2000, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                        Gender = "Male",
                        Address = "123 Student Ave, Akure",
                        Faculty = "Engineering",
                        Department = "Computer Science",
                        YearOfStudy = 3,
                        BloodGroup = "O+",
                        Genotype = "AA",
                        Allergies = "Penicillin",
                        EmergencyContactName = "Jane Doe",
                        EmergencyContactPhone = "+2348333333333",
                        IsVerified = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync(default);
                }
            }
        }

        await SeedEmailTemplatesAsync(context);
    }

    private static async Task SeedEmailTemplatesAsync(ApplicationDbContext context)
    {
        if (!await context.EmailTemplates.AnyAsync(t => t.Code == "DOCTOR_INVITATION"))
        {
            context.EmailTemplates.Add(new EmailTemplate
            {
                Id = Guid.NewGuid(),
                Code = "DOCTOR_INVITATION",
                Subject = "FUTA Medical System - Doctor Invitation",
                HtmlBody = @"<div style=""font-family: 'Outfit', 'Inter', -apple-system, sans-serif; background-color: #f8fafc; padding: 40px 20px; color: #1e293b;"">
    <div style=""max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
        <div style=""background: linear-gradient(135deg, #4f46e5 0%, #06b6d4 100%); padding: 32px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;"">FUTA Medical Booking System</h1>
        </div>
        <div style=""padding: 40px 32px;"">
            <h2 style=""margin-top: 0; font-size: 20px; font-weight: 600; color: #0f172a; line-height: 1.3;"">Doctor Account Setup Invitation</h2>
            <p style=""font-size: 15px; line-height: 1.6; color: #475569; margin: 16px 0 24px 0;"">Hello,</p>
            <p style=""font-size: 15px; line-height: 1.6; color: #475569; margin: 0 0 24px 0;"">You have been registered as a medical practitioner on the <strong>FUTA Medical Booking System</strong> platform. To activate your account and complete your onboarding, please click the secure link below to set up your password:</p>
            
            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{{SetupLink}}"" style=""display: inline-block; background-color: #4f46e5; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; box-shadow: 0 4px 10px rgba(79, 70, 229, 0.2); transition: all 0.2s ease-in-out;"">Set Up Your Password</a>
            </div>

            <p style=""font-size: 14px; line-height: 1.5; color: #64748b; margin: 24px 0 0 0; padding: 16px; background-color: #f1f5f9; border-radius: 8px; border-left: 4px solid #06b6d4;""><strong>Note:</strong> This setup link is secure and will expire in {{ExpiresIn}}.</p>
            
            <hr style=""border: none; border-top: 1px solid #e2e8f0; margin: 32px 0;"" />
            <p style=""font-size: 13px; line-height: 1.5; color: #94a3b8; margin: 0; text-align: center;"">Federal University of Technology, Akure Medical Center</p>
        </div>
    </div>
</div>"
            });
        }

        if (!await context.EmailTemplates.AnyAsync(t => t.Code == "PASSWORD_RESET"))
        {
            context.EmailTemplates.Add(new EmailTemplate
            {
                Id = Guid.NewGuid(),
                Code = "PASSWORD_RESET",
                Subject = "FUTA Medical System - Password Reset Request",
                HtmlBody = @"<div style=""font-family: 'Outfit', 'Inter', -apple-system, sans-serif; background-color: #f8fafc; padding: 40px 20px; color: #1e293b;"">
    <div style=""max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
        <div style=""background: linear-gradient(135deg, #4f46e5 0%, #06b6d4 100%); padding: 32px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;"">FUTA Medical Booking System</h1>
        </div>
        <div style=""padding: 40px 32px;"">
            <h2 style=""margin-top: 0; font-size: 20px; font-weight: 600; color: #0f172a; line-height: 1.3;"">Password Reset Request</h2>
            <p style=""font-size: 15px; line-height: 1.6; color: #475569; margin: 16px 0 24px 0;"">Hello,</p>
            <p style=""font-size: 15px; line-height: 1.6; color: #475569; margin: 0 0 24px 0;"">We received a request to reset the password for your FUTA Medical System account. Click the button below to choose a new password:</p>
            
            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{{ResetLink}}"" style=""display: inline-block; background-color: #4f46e5; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; box-shadow: 0 4px 10px rgba(79, 70, 229, 0.2); transition: all 0.2s ease-in-out;"">Reset Your Password</a>
            </div>

            <p style=""font-size: 14px; line-height: 1.5; color: #64748b; margin: 24px 0 0 0; padding: 16px; background-color: #f1f5f9; border-radius: 8px; border-left: 4px solid #f43f5e;"">If you did not make this request, you can safely ignore this email; your password will remain unchanged.</p>
            
            <hr style=""border: none; border-top: 1px solid #e2e8f0; margin: 32px 0;"" />
            <p style=""font-size: 13px; line-height: 1.5; color: #94a3b8; margin: 0; text-align: center;"">Federal University of Technology, Akure Medical Center</p>
        </div>
    </div>
</div>"
            });
        }

        await context.SaveChangesAsync(default);
    }
}
