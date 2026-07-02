using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin can access Attendance
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attendance with pagination and search
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10; // 10 rows per page

            try
            {
                IQueryable<Attendance> attendancesQuery = _context.Attendances
                    .Include(a => a.Employee)
                    .OrderBy(a => a.Date); // order by date

                // Apply search filter if searchTerm is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var normalized = searchTerm.Trim();
                    var normalizedLower = normalized.ToLower();

                    // Try parse to enum (case-insensitive)
                    bool parsed = Enum.TryParse<AttendanceStatus>(normalized, true, out var parsedStatus);

                    if (parsed)
                    {
                        attendancesQuery = attendancesQuery.Where(a =>
                            a.Employee.FullName.ToLower().Contains(normalizedLower) ||
                            a.Status == parsedStatus
                        );
                    }
                    else
                    {
                        attendancesQuery = attendancesQuery.Where(a =>
                            a.Employee.FullName.ToLower().Contains(normalizedLower)
                        );
                    }
                }

                var totalRecords = await attendancesQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var attendances = await attendancesQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["SearchTerm"] = searchTerm; // preserve search term in view

                return View(attendances);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Attendance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return RedirectToAction("Error", "Home");

                var attendance = await _context.Attendances
                                               .Include(a => a.Employee)
                                               .FirstOrDefaultAsync(m => m.Id == id);

                if (attendance == null) return RedirectToAction("Error", "Home");

                return View(attendance);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Attendance/Create
        public IActionResult Create()
        {
            try
            {
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
                ViewData["StatusList"] = new SelectList(
                    Enum.GetValues(typeof(AttendanceStatus))
                        .Cast<AttendanceStatus>()
                        .Select(e => new { Value = (int)e, Text = e.ToString() }),
                    "Value",
                    "Text");

                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Attendance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,Date,Status")] Attendance attendance)
        {
            try
            {
                ModelState.Remove("Employee");
                ModelState.Remove("AttendanceShifts");

                if (ModelState.IsValid)
                {
                    _context.Add(attendance);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendance.EmployeeId);
                ViewData["StatusList"] = new SelectList(
                    Enum.GetValues(typeof(AttendanceStatus))
                        .Cast<AttendanceStatus>()
                        .Select(e => new { Value = (int)e, Text = e.ToString() }),
                    "Value",
                    "Text",
                    (int)attendance.Status);

                return View(attendance);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Attendance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return RedirectToAction("Error", "Home");

                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null) return RedirectToAction("Error", "Home");

                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendance.EmployeeId);
                ViewData["StatusList"] = new SelectList(
                    Enum.GetValues(typeof(AttendanceStatus))
                        .Cast<AttendanceStatus>()
                        .Select(e => new { Value = (int)e, Text = e.ToString() }),
                    "Value",
                    "Text",
                    (int)attendance.Status);

                return View(attendance);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Attendance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,Date,Status")] Attendance attendance)
        {
            try
            {
                if (id != attendance.Id) return RedirectToAction("Error", "Home");

                ModelState.Remove("Employee");
                ModelState.Remove("AttendanceShifts");

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(attendance);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.Attendances.Any(e => e.Id == attendance.Id))
                            return RedirectToAction("Error", "Home");
                        else
                            throw;
                    }
                    return RedirectToAction(nameof(Index));
                }

                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", attendance.EmployeeId);
                ViewData["StatusList"] = new SelectList(
                    Enum.GetValues(typeof(AttendanceStatus))
                        .Cast<AttendanceStatus>()
                        .Select(e => new { Value = (int)e, Text = e.ToString() }),
                    "Value",
                    "Text",
                    (int)attendance.Status);

                return View(attendance);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Attendance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return RedirectToAction("Error", "Home");

                var attendance = await _context.Attendances
                                               .Include(a => a.Employee)
                                               .FirstOrDefaultAsync(a => a.Id == id);

                if (attendance == null) return RedirectToAction("Error", "Home");

                return View(attendance);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Attendance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance != null)
                {
                    _context.Attendances.Remove(attendance);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize]
        public IActionResult Test()
        {
            var claimsInfo = string.Join(", ", User.Claims.Select(c => $"{c.Type}:{c.Value}"));
            return Content($"User: {User.Identity?.Name ?? "Unknown"}\nClaims: {claimsInfo}");
        }
    }
}