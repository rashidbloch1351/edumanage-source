using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Services;

public interface IStudentService
{
    Task<StudentListViewModel> GetStudentsAsync(string? searchTerm, string? courseFilter, string? statusFilter,
        string? sortBy, string? sortDirection, int page, int pageSize);
    Task<StudentDetailsViewModel?> GetStudentByIdAsync(int id);
    Task<Student?> GetStudentEntityByIdAsync(int id);
    Task<(bool Success, string? ErrorMessage)> CreateStudentAsync(StudentCreateViewModel model);
    Task<(bool Success, string? ErrorMessage)> UpdateStudentAsync(StudentEditViewModel model);
    Task<(bool Success, string? ErrorMessage)> DeleteStudentAsync(int id);
    Task<DashboardViewModel> GetDashboardDataAsync();
    Task<bool> IsAdmissionNumberUniqueAsync(string admissionNumber, int? excludeId = null);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
    Task<List<string>> GetDistinctCoursesAsync();
    Task<List<Student>> GetAllStudentsForExportAsync(string? searchTerm, string? courseFilter, string? statusFilter);
}

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(ApplicationDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StudentListViewModel> GetStudentsAsync(string? searchTerm, string? courseFilter,
        string? statusFilter, string? sortBy, string? sortDirection, int page, int pageSize)
    {
        IQueryable<Student> query = _context.Students.AsNoTracking();

        // Apply search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                s.AdmissionNumber.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term));
        }

        // Apply course filter
        if (!string.IsNullOrWhiteSpace(courseFilter))
        {
            query = query.Where(s => s.Course == courseFilter);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<StudentStatus>(statusFilter, out var status))
        {
            query = query.Where(s => s.Status == status);
        }

        // Get total count before pagination
        var totalRecords = await query.CountAsync();

        // Apply sorting
        query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(s => s.LastName).ThenByDescending(s => s.FirstName),
            ("name", _) => query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName),
            ("admission", "desc") => query.OrderByDescending(s => s.AdmissionNumber),
            ("admission", _) => query.OrderBy(s => s.AdmissionNumber),
            ("email", "desc") => query.OrderByDescending(s => s.Email),
            ("email", _) => query.OrderBy(s => s.Email),
            ("course", "desc") => query.OrderByDescending(s => s.Course),
            ("course", _) => query.OrderBy(s => s.Course),
            ("enrollment", "desc") => query.OrderByDescending(s => s.EnrollmentDate),
            ("enrollment", _) => query.OrderBy(s => s.EnrollmentDate),
            ("status", "desc") => query.OrderByDescending(s => s.Status),
            ("status", _) => query.OrderBy(s => s.Status),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        // Apply pagination
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var students = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentListItemViewModel
            {
                Id = s.Id,
                AdmissionNumber = s.AdmissionNumber,
                FullName = s.FirstName + " " + s.LastName,
                Email = s.Email,
                Phone = s.Phone,
                Course = s.Course,
                EnrollmentDate = s.EnrollmentDate,
                Status = s.Status
            })
            .ToListAsync();

        return new StudentListViewModel
        {
            Students = students,
            SearchTerm = searchTerm,
            CourseFilter = courseFilter,
            StatusFilter = statusFilter,
            SortBy = sortBy,
            SortDirection = sortDirection,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            PageSize = pageSize,
            AvailableCourses = await GetDistinctCoursesAsync()
        };
    }

    public async Task<StudentDetailsViewModel?> GetStudentByIdAsync(int id)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentDetailsViewModel
            {
                Id = s.Id,
                AdmissionNumber = s.AdmissionNumber,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                Phone = s.Phone,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Address = s.Address,
                Course = s.Course,
                ClassName = s.ClassName,
                EnrollmentDate = s.EnrollmentDate,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return student;
    }

    public async Task<Student?> GetStudentEntityByIdAsync(int id)
    {
        return await _context.Students.FindAsync(id);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateStudentAsync(StudentCreateViewModel model)
    {
        try
        {
            // Check unique constraints
            if (!await IsAdmissionNumberUniqueAsync(model.AdmissionNumber))
            {
                return (false, "A student with this admission number already exists.");
            }

            if (!await IsEmailUniqueAsync(model.Email))
            {
                return (false, "A student with this email address already exists.");
            }

            var student = new Student
            {
                AdmissionNumber = model.AdmissionNumber.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                NormalizedEmail = model.Email.Trim().ToUpperInvariant(),
                Phone = model.Phone?.Trim(),
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = model.Address?.Trim(),
                Course = model.Course.Trim(),
                ClassName = model.ClassName?.Trim(),
                EnrollmentDate = model.EnrollmentDate,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student created successfully: {AdmissionNumber}", student.AdmissionNumber);
            return (true, null);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Duplicate key violation when creating student");
            return (false, "A student with this admission number or email already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return (false, "An unexpected error occurred while creating the student. Please try again.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateStudentAsync(StudentEditViewModel model)
    {
        try
        {
            var student = await _context.Students.FindAsync(model.Id);
            if (student == null)
            {
                return (false, "Student not found. The record may have been deleted.");
            }

            // Check unique constraints excluding current record
            if (!await IsAdmissionNumberUniqueAsync(model.AdmissionNumber, model.Id))
            {
                return (false, "Another student with this admission number already exists.");
            }

            if (!await IsEmailUniqueAsync(model.Email, model.Id))
            {
                return (false, "Another student with this email address already exists.");
            }

            // Map permitted fields only
            student.AdmissionNumber = model.AdmissionNumber.Trim();
            student.FirstName = model.FirstName.Trim();
            student.LastName = model.LastName.Trim();
            student.Email = model.Email.Trim();
            student.NormalizedEmail = model.Email.Trim().ToUpperInvariant();
            student.Phone = model.Phone?.Trim();
            student.DateOfBirth = model.DateOfBirth;
            student.Gender = model.Gender;
            student.Address = model.Address?.Trim();
            student.Course = model.Course.Trim();
            student.ClassName = model.ClassName?.Trim();
            student.EnrollmentDate = model.EnrollmentDate;
            student.Status = model.Status;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Student updated successfully: {AdmissionNumber}", student.AdmissionNumber);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating student {Id}", model.Id);
            return (false, "The student record was modified by another user. Please reload and try again.");
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Duplicate key violation when updating student {Id}", model.Id);
            return (false, "Another student with this admission number or email already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {Id}", model.Id);
            return (false, "An unexpected error occurred while updating the student. Please try again.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteStudentAsync(int id)
    {
        try
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return (false, "Student not found. The record may have already been deleted.");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student deleted successfully: {AdmissionNumber}", student.AdmissionNumber);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {Id}", id);
            return (false, "An unexpected error occurred while deleting the student. Please try again.");
        }
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var totalStudents = await _context.Students.CountAsync();
        var activeStudents = await _context.Students.CountAsync(s => s.Status == StudentStatus.Active);
        var inactiveStudents = await _context.Students.CountAsync(s => s.Status == StudentStatus.Inactive);
        var graduatedStudents = await _context.Students.CountAsync(s => s.Status == StudentStatus.Graduated);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentEnrollments = await _context.Students
            .CountAsync(s => s.EnrollmentDate >= thirtyDaysAgo);

        var studentsByCourse = await _context.Students
            .GroupBy(s => s.Course)
            .Select(g => new CourseGroupViewModel
            {
                Course = g.Key,
                Count = g.Count(),
                Percentage = totalStudents > 0 ? Math.Round(g.Count() * 100.0 / totalStudents, 1) : 0
            })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var studentsByStatus = new List<StatusGroupViewModel>();
        if (totalStudents > 0)
        {
            studentsByStatus.Add(new StatusGroupViewModel
            {
                Status = "Active",
                Count = activeStudents,
                Percentage = Math.Round(activeStudents * 100.0 / totalStudents, 1),
                CssClass = "success"
            });
            studentsByStatus.Add(new StatusGroupViewModel
            {
                Status = "Inactive",
                Count = inactiveStudents,
                Percentage = Math.Round(inactiveStudents * 100.0 / totalStudents, 1),
                CssClass = "warning"
            });
            studentsByStatus.Add(new StatusGroupViewModel
            {
                Status = "Graduated",
                Count = graduatedStudents,
                Percentage = Math.Round(graduatedStudents * 100.0 / totalStudents, 1),
                CssClass = "info"
            });
        }

        var recentStudents = await _context.Students
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new RecentStudentViewModel
            {
                Id = s.Id,
                FullName = s.FirstName + " " + s.LastName,
                AdmissionNumber = s.AdmissionNumber,
                Course = s.Course,
                EnrollmentDate = s.EnrollmentDate,
                Status = s.Status.ToString()
            })
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            InactiveStudents = inactiveStudents,
            GraduatedStudents = graduatedStudents,
            RecentEnrollments = recentEnrollments,
            StudentsByCourse = studentsByCourse,
            StudentsByStatus = studentsByStatus,
            RecentStudents = recentStudents
        };
    }

    public async Task<bool> IsAdmissionNumberUniqueAsync(string admissionNumber, int? excludeId = null)
    {
        var trimmed = admissionNumber.Trim();
        var query = _context.Students.Where(s => s.AdmissionNumber == trimmed);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var query = _context.Students.Where(s => s.NormalizedEmail == normalizedEmail);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }

    public async Task<List<string>> GetDistinctCoursesAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .Select(s => s.Course)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<Student>> GetAllStudentsForExportAsync(string? searchTerm, string? courseFilter, string? statusFilter)
    {
        IQueryable<Student> query = _context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                s.AdmissionNumber.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(courseFilter))
        {
            query = query.Where(s => s.Course == courseFilter);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<StudentStatus>(statusFilter, out var status))
        {
            query = query.Where(s => s.Status == status);
        }

        return await query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync();
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true ||
               ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
               ex.InnerException?.Message.Contains("IX_", StringComparison.OrdinalIgnoreCase) == true;
    }
}
