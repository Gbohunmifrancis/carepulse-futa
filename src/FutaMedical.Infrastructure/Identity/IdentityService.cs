using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FutaMedical.Application.Common.Interfaces;
using FutaMedical.Application.Common.Models;
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

    public async Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

        if (user == null || !BCryptLib.Verify(password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.BadRequest("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.BadRequest("Account is suspended");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
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
        }, "Login successful");
    }

    public async Task<ApiResponse<AuthResponse>> RegisterStudentAsync(RegisterStudentRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return ApiResponse<AuthResponse>.BadRequest("Email already exists");
        }

        if (await _context.Students.AnyAsync(s => s.MatricNumber == request.MatricNumber, cancellationToken))
        {
            return ApiResponse<AuthResponse>.BadRequest("Matric number already exists");
        }

        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student", cancellationToken);
        if (studentRole == null)
        {
            return ApiResponse<AuthResponse>.BadRequest("Student role not found in system");
        }

        // Use execution strategy to wrap the transaction for retry support
        var strategy = ((DbContext)_context).Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync(cancellationToken);
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

                var roles = new List<string> { "Student" };
                var token = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                _context.Users.Add(user);
                _context.Students.Add(student);
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ApiResponse<AuthResponse>.Ok(new AuthResponse
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
                }, "Registration successful");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ApiResponse<AuthResponse>.BadRequest($"An error occurred during registration: {ex.Message}");
            }
        });
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return ApiResponse<AuthResponse>.BadRequest("Invalid token");
        }

        var email = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value 
                    ?? principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.BadRequest("Invalid refresh token");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newToken = _jwtService.GenerateToken(user.Id, user.Email, user.FirstName, user.LastName, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
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
        }, "Token refreshed successfully");
    }
}
