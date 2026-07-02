using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize] // all actions require login
    public class SalaryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const decimal ShiftRate = 300m;
        private const decimal AbsenceDeductionPercent = 0.05m; // 5% deduction per absent shift

        public SalaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Guards and Admins: Can access Index with pagination and search
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10; // show 10 rows per page

            try
            {
                // Explicitly declare IQueryable to avoid CS0266
                IQueryable<Salary> salariesQuery = _context.Salaries
                                                          .Include(s => s.Employee)
                                                          .OrderBy(s => s.Id); // keep consistent order

                // Apply search filter if searchTerm is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    salariesQuery = salariesQuery.Where(s =>
                        s.Employee.FullName.Contains(searchTerm) ||
                        s.Employee.Phone.Contains(searchTerm)
                    );
                }

                var totalRecords = await salariesQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var salaries = await salariesQuery
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["SearchTerm"] = searchTerm; // preserve search term in view

                return View(salaries);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Guards and Admins: Can access Details
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var salary = await _context.Salaries
                                           .Include(s => s.Employee)
                                           .FirstOrDefaultAsync(s => s.Id == id);

                if (salary == null) return RedirectToAction("Error", "Home");

                // Get all attendances for this employee and month/year
                var attendances = await _context.Attendances
                                                .Where(a => a.EmployeeId == salary.EmployeeId &&
                                                            a.Date.Month == salary.Month &&
                                                            a.Date.Year == salary.Year)
                                                .ToListAsync();

                salary.TotalShifts = attendances.Count;
                salary.AbsentShifts = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                salary.EarningBeforeDeduction = salary.TotalShifts * ShiftRate;

                // Deduction based on absent shifts
                var deductionAmount = salary.EarningBeforeDeduction * AbsenceDeductionPercent * salary.AbsentShifts;

                // Total salary after deduction
                salary.TotalSalary = salary.EarningBeforeDeduction - deductionAmount;

                // Pass deduction amount to ViewData for display
                ViewData["DeductionAmount"] = deductionAmount;

                return View(salary);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Only Admins: Create Salary
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            try
            {
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Only Admins: Create Salary POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EmployeeId,Month,Year")] Salary salary)
        {
            try
            {
                // Remove navigation-property validation errors
                ModelState.Remove("Employee");
                ModelState.Remove("TotalShifts");
                ModelState.Remove("AbsentShifts");
                ModelState.Remove("EarningBeforeDeduction");
                ModelState.Remove("TotalSalary");

                if (salary.Month < 1 || salary.Month > 12)
                    ModelState.AddModelError("Month", "Month must be between 1 and 12.");

                if (salary.Year < 2000 || salary.Year > 2100)
                    ModelState.AddModelError("Year", "Year must be between 2000 and 2100.");

                if (ModelState.IsValid)
                {
                    // Calculate salary dynamically
                    var attendances = await _context.Attendances
                                                    .Where(a => a.EmployeeId == salary.EmployeeId &&
                                                                a.Date.Month == salary.Month &&
                                                                a.Date.Year == salary.Year)
                                                    .ToListAsync();

                    salary.TotalShifts = attendances.Count;
                    salary.AbsentShifts = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                    salary.EarningBeforeDeduction = salary.TotalShifts * ShiftRate;
                    salary.TotalSalary = salary.EarningBeforeDeduction * (1 - AbsenceDeductionPercent * salary.AbsentShifts);

                    _context.Add(salary);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Repopulate Employee dropdown if validation fails
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", salary.EmployeeId);
                return View(salary);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Only Admins: Edit Salary
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var salary = await _context.Salaries
                                           .Include(s => s.Employee)
                                           .FirstOrDefaultAsync(s => s.Id == id);

                if (salary == null) return RedirectToAction("Error", "Home");

                // Populate employee dropdown again
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", salary.EmployeeId);
                return View(salary);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // Only Admins: Edit Salary POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,Month,Year")] Salary salary)
        {
            if (id != salary.Id) return RedirectToAction("Error", "Home");

            try
            {
                // Remove navigation-property validation errors
                ModelState.Remove("Employee");
                ModelState.Remove("TotalShifts");
                ModelState.Remove("AbsentShifts");
                ModelState.Remove("EarningBeforeDeduction");
                ModelState.Remove("TotalSalary");

                if (salary.Month < 1 || salary.Month > 12)
                    ModelState.AddModelError("Month", "Month must be between 1 and 12.");

                if (salary.Year < 2000 || salary.Year > 2100)
                    ModelState.AddModelError("Year", "Year must be between 2000 and 2100.");

                if (!ModelState.IsValid)
                {
                    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", salary.EmployeeId);
                    return View(salary);
                }

                // Recalculate salary with updated Month/Year
                var attendances = await _context.Attendances
                                                .Where(a => a.EmployeeId == salary.EmployeeId &&
                                                            a.Date.Month == salary.Month &&
                                                            a.Date.Year == salary.Year)
                                                .ToListAsync();

                salary.TotalShifts = attendances.Count;
                salary.AbsentShifts = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                salary.EarningBeforeDeduction = salary.TotalShifts * ShiftRate;
                salary.TotalSalary = salary.EarningBeforeDeduction * (1 - AbsenceDeductionPercent * salary.AbsentShifts);

                _context.Update(salary);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Salaries.Any(e => e.Id == salary.Id))
                    return RedirectToAction("Error", "Home");
                else
                    throw;
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}