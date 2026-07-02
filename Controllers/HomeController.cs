using System.Diagnostics;
using Employee_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize] // Only logged-in users can access the dashboard
        public IActionResult Dashboard()
        {
            // You can pass role info to the view via ViewData or ViewBag
            if (User.IsInRole("Admin"))
            {
                ViewData["Role"] = "Admin";
            }
            else if (User.IsInRole("Guard"))
            {
                ViewData["Role"] = "Guard";
            }
            else
            {
                ViewData["Role"] = "Unknown";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string? message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                _logger.LogError("Error occurred: {Message}", message);
            }

            ViewData["ErrorMessage"] = message ?? "An unexpected error occurred. Please try again.";

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}