using StudentManagementSystem.Models;

namespace StudentManagementSystem.ViewModels;

public class StudentListViewModel
{
    public List<StudentListItemViewModel> Students { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? CourseFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public int PageSize { get; set; } = 10;
    public List<string> AvailableCourses { get; set; } = new();
}

public class StudentListItemViewModel
{
    public int Id { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Course { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; }
}
