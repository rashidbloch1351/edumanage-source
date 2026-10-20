using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

public class StudentDetailsViewModel
{
    public int Id { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public string Course { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
