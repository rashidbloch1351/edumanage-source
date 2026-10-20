using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers;

[Authorize(Roles = "Administrator")]
public class StudentsController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    // GET: Students
    public async Task<IActionResult> Index(string? searchTerm, string? courseFilter, string? statusFilter,
        string? sortBy, string? sortDirection, int page = 1, int pageSize = 10)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 5, 50);
            var viewModel = await _studentService.GetStudentsAsync(
                searchTerm, courseFilter, statusFilter, sortBy, sortDirection, page, pageSize);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading students list");
            TempData["Error"] = "Unable to load student records. Please try again.";
            return View(new StudentListViewModel());
        }
    }

    // GET: Students/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _studentService.GetStudentByIdAsync(id.Value);
        if (student == null)
        {
            TempData["Error"] = "Student not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(student);
    }

    // GET: Students/Create
    public IActionResult Create()
    {
        var model = new StudentCreateViewModel
        {
            EnrollmentDate = DateTime.Today,
            Status = StudentStatus.Active
        };
        return View(model);
    }

    // POST: Students/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage) = await _studentService.CreateStudentAsync(model);

        if (success)
        {
            TempData["Success"] = $"Student '{model.FirstName} {model.LastName}' has been created successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred.");
        return View(model);
    }

    // GET: Students/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _studentService.GetStudentEntityByIdAsync(id.Value);
        if (student == null)
        {
            TempData["Error"] = "Student not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new StudentEditViewModel
        {
            Id = student.Id,
            AdmissionNumber = student.AdmissionNumber,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            Phone = student.Phone,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Address = student.Address,
            Course = student.Course,
            ClassName = student.ClassName,
            EnrollmentDate = student.EnrollmentDate,
            Status = student.Status,
            CreatedAt = student.CreatedAt
        };

        return View(model);
    }

    // POST: Students/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage) = await _studentService.UpdateStudentAsync(model);

        if (success)
        {
            TempData["Success"] = $"Student '{model.FirstName} {model.LastName}' has been updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred.");
        return View(model);
    }

    // GET: Students/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _studentService.GetStudentByIdAsync(id.Value);
        if (student == null)
        {
            TempData["Error"] = "Student not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(student);
    }

    // POST: Students/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (success, errorMessage) = await _studentService.DeleteStudentAsync(id);

        if (success)
        {
            TempData["Success"] = "Student has been deleted successfully.";
        }
        else
        {
            TempData["Error"] = errorMessage ?? "An error occurred while deleting the student.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Students/Export
    public async Task<IActionResult> Export(string? searchTerm, string? courseFilter, string? statusFilter)
    {
        try
        {
            var students = await _studentService.GetAllStudentsForExportAsync(searchTerm, courseFilter, statusFilter);

            var sb = new StringBuilder();
            sb.AppendLine("Admission Number,First Name,Last Name,Email,Phone,Date of Birth,Gender,Address,Course,Class,Enrollment Date,Status");

            foreach (var s in students)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCsv(s.AdmissionNumber),
                    EscapeCsv(s.FirstName),
                    EscapeCsv(s.LastName),
                    EscapeCsv(s.Email),
                    EscapeCsv(s.Phone ?? ""),
                    s.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                    s.Gender?.ToString() ?? "",
                    EscapeCsv(s.Address ?? ""),
                    EscapeCsv(s.Course),
                    EscapeCsv(s.ClassName ?? ""),
                    s.EnrollmentDate.ToString("yyyy-MM-dd"),
                    s.Status.ToString()));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"students_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting student data");
            TempData["Error"] = "Unable to export student data. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Escapes a CSV value to prevent formula injection and handle special characters.
    /// </summary>
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        // Prevent formula injection by prefixing dangerous characters
        if (value.Length > 0 && "=+-@\t\r".Contains(value[0]))
        {
            value = "'" + value;
        }

        // Always quote and escape internal quotes
        value = value.Replace("\"", "\"\"");
        return $"\"{value}\"";
    }
}
