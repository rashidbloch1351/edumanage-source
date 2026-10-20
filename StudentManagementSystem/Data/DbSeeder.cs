using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Create Administrator role
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("Administrator"));
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Administrator role created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create Administrator role: {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    return;
                }
            }

            // Create default administrator (development only)
            var adminEmail = "admin@edumanage.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                // NOTE: Change this password immediately in production!
                var createResult = await userManager.CreateAsync(adminUser, "Admin@123456");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    logger.LogInformation("Default administrator account created. Email: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create default administrator: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            // Seed sample students if none exist
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (!dbContext.Students.Any())
            {
                var sampleStudents = new List<Student>
                {
                    new()
                    {
                        AdmissionNumber = "STD-2026-001",
                        FirstName = "Alexander",
                        LastName = "Wright",
                        Email = "alexander.wright@student.edu",
                        NormalizedEmail = "ALEXANDER.WRIGHT@STUDENT.EDU",
                        Phone = "+1 (555) 234-5678",
                        DateOfBirth = new DateTime(2003, 4, 15),
                        Gender = Gender.Male,
                        Address = "742 Evergreen Terrace, Springfield",
                        Course = "Computer Science",
                        ClassName = "CS-Year-3",
                        EnrollmentDate = new DateTime(2023, 9, 1),
                        Status = StudentStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        AdmissionNumber = "STD-2026-002",
                        FirstName = "Sophia",
                        LastName = "Chen",
                        Email = "sophia.chen@student.edu",
                        NormalizedEmail = "SOPHIA.CHEN@STUDENT.EDU",
                        Phone = "+1 (555) 345-6789",
                        DateOfBirth = new DateTime(2004, 8, 22),
                        Gender = Gender.Female,
                        Address = "10880 Wilshire Blvd, Los Angeles, CA",
                        Course = "Data Science",
                        ClassName = "DS-Year-2",
                        EnrollmentDate = new DateTime(2024, 9, 1),
                        Status = StudentStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        AdmissionNumber = "STD-2026-003",
                        FirstName = "Marcus",
                        LastName = "Johnson",
                        Email = "marcus.johnson@student.edu",
                        NormalizedEmail = "MARCUS.JOHNSON@STUDENT.EDU",
                        Phone = "+1 (555) 456-7890",
                        DateOfBirth = new DateTime(2002, 11, 5),
                        Gender = Gender.Male,
                        Address = "350 5th Ave, New York, NY",
                        Course = "Software Engineering",
                        ClassName = "SE-Year-4",
                        EnrollmentDate = new DateTime(2022, 9, 1),
                        Status = StudentStatus.Graduated,
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        AdmissionNumber = "STD-2026-004",
                        FirstName = "Elena",
                        LastName = "Rostova",
                        Email = "elena.rostova@student.edu",
                        NormalizedEmail = "ELENA.ROSTOVA@STUDENT.EDU",
                        Phone = "+1 (555) 567-8901",
                        DateOfBirth = new DateTime(2005, 1, 30),
                        Gender = Gender.Female,
                        Address = "450 Serra Mall, Stanford, CA",
                        Course = "Cybersecurity",
                        ClassName = "CY-Year-1",
                        EnrollmentDate = new DateTime(2025, 9, 1),
                        Status = StudentStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        AdmissionNumber = "STD-2026-005",
                        FirstName = "Liam",
                        LastName = "O'Connor",
                        Email = "liam.oconnor@student.edu",
                        NormalizedEmail = "LIAM.OCONNOR@STUDENT.EDU",
                        Phone = "+1 (555) 678-9012",
                        DateOfBirth = new DateTime(2003, 6, 12),
                        Gender = Gender.Male,
                        Address = "123 Main St, Boston, MA",
                        Course = "Information Technology",
                        ClassName = "IT-Year-3",
                        EnrollmentDate = new DateTime(2023, 9, 1),
                        Status = StudentStatus.Inactive,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                dbContext.Students.AddRange(sampleStudents);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Sample student records seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
