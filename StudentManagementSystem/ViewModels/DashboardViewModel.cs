namespace StudentManagementSystem.ViewModels;

public class DashboardViewModel
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }
    public int GraduatedStudents { get; set; }
    public int RecentEnrollments { get; set; }
    public List<CourseGroupViewModel> StudentsByCourse { get; set; } = new();
    public List<StatusGroupViewModel> StudentsByStatus { get; set; } = new();
    public List<RecentStudentViewModel> RecentStudents { get; set; } = new();
}

public class CourseGroupViewModel
{
    public string Course { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class StatusGroupViewModel
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public string CssClass { get; set; } = string.Empty;
}

public class RecentStudentViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
