using FutaMedical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutaMedical.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.MatricNumber).IsUnique();
        builder.Property(s => s.MatricNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Gender).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Faculty).HasMaxLength(100);
        builder.Property(s => s.Department).HasMaxLength(100);
        builder.Property(s => s.BloodGroup).HasMaxLength(5);
        builder.Property(s => s.Genotype).HasMaxLength(5);
        
        builder.HasOne(s => s.User)
               .WithOne(u => u.Student)
               .HasForeignKey<Student>(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.LicenseNumber).IsUnique();
        builder.Property(d => d.Specialization).IsRequired().HasMaxLength(100);
        builder.Property(d => d.LicenseNumber).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Rating).HasPrecision(2, 1);
        
        builder.HasOne(d => d.User)
               .WithOne(u => u.Doctor)
               .HasForeignKey<Doctor>(d => d.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(d => d.Department)
               .WithMany(dept => dept.Doctors)
               .HasForeignKey(d => d.DepartmentId);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => ur.Id);
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        
        builder.HasOne(ur => ur.User)
               .WithMany(u => u.UserRoles)
               .HasForeignKey(ur => ur.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ur => ur.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(ur => ur.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.HasKey(da => da.Id);
        builder.HasIndex(da => new { da.DoctorId, da.DayOfWeek }).IsUnique();
        
        builder.HasOne(da => da.Doctor)
               .WithMany(d => d.Availabilities)
               .HasForeignKey(da => da.DoctorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
