using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers;

[Authorize(Roles = "Administrator")]
public class DashboardController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IStudentService studentService, ILogger<DashboardController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboardData = await _studentService.GetDashboardDataAsync();
            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            TempData["Error"] = "Unable to load dashboard data. Please try again.";
            return View(new ViewModels.DashboardViewModel());
        }
    }
}
