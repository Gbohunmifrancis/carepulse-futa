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
        builder.Property(d => d.Specialization).HasMaxLength(100);
        builder.Property(d => d.LicenseNumber).HasMaxLength(50);
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

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(us => us.Id);
        builder.HasIndex(us => us.TokenJti).IsUnique();
        builder.Property(us => us.TokenJti).IsRequired().HasMaxLength(100);
        builder.Property(us => us.UserAgent).HasMaxLength(500);
        builder.Property(us => us.IpAddress).HasMaxLength(50);
        
        builder.HasOne(us => us.User)
               .WithMany()
               .HasForeignKey(us => us.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.HasKey(et => et.Id);
        builder.HasIndex(et => et.Code).IsUnique();
        builder.Property(et => et.Code).IsRequired().HasMaxLength(100);
        builder.Property(et => et.Subject).IsRequired().HasMaxLength(200);
        builder.Property(et => et.HtmlBody).IsRequired();
    }
}

public class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.HasKey(eq => eq.Id);
        builder.Property(eq => eq.To).IsRequired().HasMaxLength(255);
        builder.Property(eq => eq.Subject).IsRequired().HasMaxLength(200);
        builder.Property(eq => eq.Body).IsRequired();
        builder.Property(eq => eq.TemplateCode).HasMaxLength(100);
        builder.Property(eq => eq.Status).IsRequired().HasMaxLength(50);
        builder.Property(eq => eq.ErrorMessage).HasMaxLength(2000);
    }
}

public class DoctorLeaveRequestConfiguration : IEntityTypeConfiguration<DoctorLeaveRequest>
{
    public void Configure(EntityTypeBuilder<DoctorLeaveRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(30);
        builder.Property(x => x.AdminResponse).HasMaxLength(1000);

        builder.HasOne(x => x.Doctor)
            .WithMany(d => d.LeaveRequests)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HealthArticleConfiguration : IEntityTypeConfiguration<HealthArticle>
{
    public void Configure(EntityTypeBuilder<HealthArticle> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Summary).IsRequired().HasMaxLength(1200);
        builder.Property(x => x.Content).IsRequired();
    }
}

public class VaccinationRecordConfiguration : IEntityTypeConfiguration<VaccinationRecord>
{
    public void Configure(EntityTypeBuilder<VaccinationRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VaccineName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Provider).HasMaxLength(200);
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.VaccinationRecords)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WaitingListEntryConfiguration : IEntityTypeConfiguration<WaitingListEntry>
{
    public void Configure(EntityTypeBuilder<WaitingListEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(30);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.WaitingListEntries)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Doctor)
            .WithMany(d => d.WaitingListEntries)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PrescriptionTemplateConfiguration : IEntityTypeConfiguration<PrescriptionTemplate>
{
    public void Configure(EntityTypeBuilder<PrescriptionTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.MedicationName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Frequency).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Duration).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Instructions).HasMaxLength(500);

        builder.HasOne(x => x.Doctor)
            .WithMany(d => d.PrescriptionTemplates)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HospitalName).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Doctor)
            .WithMany(d => d.Referrals)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.Referrals)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
