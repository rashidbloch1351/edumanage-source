using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.AdmissionNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasIndex(e => e.AdmissionNumber)
                .IsUnique()
                .HasDatabaseName("IX_Students_AdmissionNumber");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(254);

            entity.Property(e => e.NormalizedEmail)
                .HasMaxLength(254);

            entity.HasIndex(e => e.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_Students_NormalizedEmail");

            entity.Property(e => e.Phone)
                .HasMaxLength(25);

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.Course)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ClassName)
                .HasMaxLength(50);

            entity.Property(e => e.Gender)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
            else
            {
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            }

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Students_Status");

            entity.HasIndex(e => e.Course)
                .HasDatabaseName("IX_Students_Course");

            entity.HasIndex(e => e.EnrollmentDate)
                .HasDatabaseName("IX_Students_EnrollmentDate");
        });
    }
}
