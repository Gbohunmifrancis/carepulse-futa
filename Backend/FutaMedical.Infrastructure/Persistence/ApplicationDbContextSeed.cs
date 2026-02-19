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
        if (adminRole != null && !await context.Users.AnyAsync(u => u.Email == "admin@futa.edu.ng"))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@futa.edu.ng",
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

        // Seed Sample Doctor (Optional for testing)
        var doctorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
        var generalMedicineDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "General Medicine");
        
        if (doctorRole != null && generalMedicineDept != null && !await context.Users.AnyAsync(u => u.Email == "doctor@futa.edu.ng"))
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

        // Seed Sample Student (Optional for testing)
        var studentRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        
        if (studentRole != null && !await context.Users.AnyAsync(u => u.Email == "student@futa.edu.ng"))
        {
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
                DateOfBirth = new DateTime(2000, 5, 15),
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
