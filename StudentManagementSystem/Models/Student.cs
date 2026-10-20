using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models;

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}

public enum StudentStatus
{
    Active,
    Inactive,
    Graduated
}

public class Student
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Admission number is required.")]
    [StringLength(30, ErrorMessage = "Admission number cannot exceed 30 characters.")]
    [Display(Name = "Admission Number")]
    public string AdmissionNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(254)]
    public string NormalizedEmail { get; set; } = string.Empty;

    [StringLength(25, ErrorMessage = "Phone number cannot exceed 25 characters.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Gender")]
    public Gender? Gender { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [StringLength(100, ErrorMessage = "Course cannot exceed 100 characters.")]
    [Display(Name = "Course")]
    public string Course { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Class name cannot exceed 50 characters.")]
    [Display(Name = "Class")]
    public string? ClassName { get; set; }

    [Required(ErrorMessage = "Enrollment date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Enrollment Date")]
    public DateTime EnrollmentDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";
}
