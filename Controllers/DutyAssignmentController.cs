using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace Employee_Management_System.Controllers
{
    [Authorize] // Require login for everything
    public class DutyAssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DutyAssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DutyAssignment with pagination and search
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10; // 10 rows per page

            try
            {
                // Explicitly declare IQueryable to avoid CS0266
                IQueryable<DutyAssignment> assignmentsQuery = _context.DutyAssignments
                    .Include(d => d.Client)
                    .Include(d => d.Employee)
                    .Include(d => d.Shift)
                    .OrderBy(d => d.Date)
                    .ThenBy(d => d.ShiftId);

                // Apply search filter if searchTerm is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    assignmentsQuery = assignmentsQuery.Where(d =>
                        d.Employee.FullName.Contains(searchTerm) ||
                        d.Client.Name.Contains(searchTerm)
                    );
                }

                var totalRecords = await assignmentsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var assignments = await assignmentsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["SearchTerm"] = searchTerm; // preserve search term in view

                return View(assignments);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: DutyAssignment/Details/5
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var assignment = await _context.DutyAssignments
                    .Include(d => d.Client)
                    .Include(d => d.Employee)
                    .Include(d => d.Shift)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (assignment == null) return RedirectToAction("Error", "Home");

                return View(assignment);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: DutyAssignment/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            try
            {
                PopulateDropdowns();
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: DutyAssignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DutyAssignment dutyAssignment)
        {
            try
            {
                ModelState.Remove("Employee");
                ModelState.Remove("Client");
                ModelState.Remove("Shift");

                PopulateDropdowns(dutyAssignment);

                if (!ModelState.IsValid)
                    return View(dutyAssignment);

                var postedShift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == dutyAssignment.ShiftId);
                if (postedShift == null)
                {
                    ModelState.AddModelError(nameof(DutyAssignment.ShiftId), "Selected shift not found.");
                    return View(dutyAssignment);
                }

                if (IsShiftOverlapping(dutyAssignment.EmployeeId, dutyAssignment.Date, postedShift.StartTime, postedShift.EndTime))
                {
                    ModelState.AddModelError("", "This employee already has a shift that overlaps on the selected date.");
                    return View(dutyAssignment);
                }

                _context.Add(dutyAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: DutyAssignment/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var dutyAssignment = await _context.DutyAssignments.FindAsync(id);
                if (dutyAssignment == null) return RedirectToAction("Error", "Home");

                PopulateDropdowns(dutyAssignment);
                return View(dutyAssignment);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: DutyAssignment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, DutyAssignment dutyAssignment)
        {
            if (id != dutyAssignment.Id) return RedirectToAction("Error", "Home");

            try
            {
                ModelState.Remove("Employee");
                ModelState.Remove("Client");
                ModelState.Remove("Shift");

                PopulateDropdowns(dutyAssignment);

                if (!ModelState.IsValid)
                    return View(dutyAssignment);

                var postedShift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == dutyAssignment.ShiftId);
                if (postedShift == null)
                {
                    ModelState.AddModelError(nameof(DutyAssignment.ShiftId), "Selected shift not found.");
                    return View(dutyAssignment);
                }

                if (IsShiftOverlapping(dutyAssignment.EmployeeId, dutyAssignment.Date, postedShift.StartTime, postedShift.EndTime, dutyAssignment.Id))
                {
                    ModelState.AddModelError("", "This employee already has a shift that overlaps on the selected date.");
                    return View(dutyAssignment);
                }

                _context.Update(dutyAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DutyAssignmentExists(dutyAssignment.Id))
                    return RedirectToAction("Error", "Home");
                else
                    throw;
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: DutyAssignment/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var assignment = await _context.DutyAssignments
                    .Include(d => d.Client)
                    .Include(d => d.Employee)
                    .Include(d => d.Shift)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (assignment == null) return RedirectToAction("Error", "Home");

                return View(assignment);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: DutyAssignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var assignment = await _context.DutyAssignments.FindAsync(id);
                if (assignment != null)
                {
                    _context.DutyAssignments.Remove(assignment);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        private bool DutyAssignmentExists(int id) => _context.DutyAssignments.Any(e => e.Id == id);

        private void PopulateDropdowns(DutyAssignment? dutyAssignment = null)
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.Position == EmployeePosition.Guard)
                .ToList(), "Id", "FullName", dutyAssignment?.EmployeeId);

            ViewData["ClientId"] = new SelectList(_context.Clients.ToList(), "Id", "Name", dutyAssignment?.ClientId);

            ViewData["ShiftId"] = new SelectList(_context.Shifts.ToList(), "Id", "Name", dutyAssignment?.ShiftId);
        }

        private bool IsShiftOverlapping(int employeeId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var employeeShifts = _context.DutyAssignments
                .Include(d => d.Shift)
                .Where(d => d.EmployeeId == employeeId && d.Date.Date == date.Date);

            if (excludeId.HasValue)
                employeeShifts = employeeShifts.Where(d => d.Id != excludeId.Value);

            foreach (var existing in employeeShifts)
            {
                if (startTime < existing.Shift.EndTime && existing.Shift.StartTime < endTime)
                    return true;
            }

            return false;
        }
    }
}