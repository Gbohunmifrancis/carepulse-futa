using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Features.Auth.DTOs;
using FutaMedical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BCryptLib = BCrypt.Net.BCrypt;

namespace FutaMedical.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public IdentityService(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<(bool Success, string Message, AuthResponse? Response)> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

        if (user == null || !BCryptLib.Verify(password, user.PasswordHash))
        {
            return (false, "Invalid email or password", null);
        }

        if (!user.IsActive)
        {
            return (false, "Account is suspended", null);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(default);

        return (true, "Login successful", new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            }
        });
    }

    public async Task<(bool Success, string Message, AuthResponse? Response)> RegisterStudentAsync(RegisterStudentRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return (false, "Email already exists", null);
        }

        if (await _context.Students.AnyAsync(s => s.MatricNumber == request.MatricNumber))
        {
            return (false, "Matric number already exists", null);
        }

        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole == null)
        {
            return (false, "Student role not found in system", null);
        }

        using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCryptLib.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
            };

            user.UserRoles.Add(new UserRole { RoleId = studentRole.Id });
            _context.Users.Add(user);
            await _context.SaveChangesAsync(default);

            var student = new Student
            {
                UserId = user.Id,
                MatricNumber = request.MatricNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                Faculty = request.Faculty,
                Department = request.Department,
                YearOfStudy = request.YearOfStudy,
                BloodGroup = request.BloodGroup,
                Genotype = request.Genotype,
                Allergies = request.Allergies,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync(default);
            await transaction.CommitAsync();

            var roles = new List<string> { "Student" };
            var token = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync(default);

            return (true, "Registration successful", new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"An error occurred during registration: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, AuthResponse? Response)> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return (false, "Invalid token", null);
        }

        var email = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value 
                    ?? principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return (false, "Invalid refresh token", null);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newToken = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _context.SaveChangesAsync(default);

        return (true, "Token refreshed successfully", new AuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            }
        });
    }
}
