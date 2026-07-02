using System;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Employee_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can access
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee with pagination and search
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10; // 10 rows per page

            try
            {
                // Explicitly declare IQueryable to avoid CS0266
                IQueryable<Employee> employeesQuery = _context.Employees.OrderBy(e => e.Id);

                // Apply search filter if searchTerm is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    employeesQuery = employeesQuery.Where(e =>
                        e.FullName.Contains(searchTerm) ||
                        e.Phone.Contains(searchTerm) ||
                        (e.Address != null && e.Address.Contains(searchTerm))
                    );
                }

                var totalRecords = await employeesQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var employees = await employeesQuery
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["SearchTerm"] = searchTerm; // preserve search term in view

                return View(employees);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
                if (employee == null) return RedirectToAction("Error", "Home");

                return View(employee);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            try
            {
                ViewBag.PositionList = new SelectList(Enum.GetValues(typeof(EmployeePosition)));
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Phone,Address,Position,JoiningDate")] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PositionList = new SelectList(Enum.GetValues(typeof(EmployeePosition)), employee.Position);
                return View(employee);
            }

            try
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null) return RedirectToAction("Error", "Home");

                ViewBag.PositionList = new SelectList(Enum.GetValues(typeof(EmployeePosition)), employee.Position);
                return View(employee);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Phone,Address,Position,JoiningDate")] Employee employee)
        {
            if (id != employee.Id) return RedirectToAction("Error", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.PositionList = new SelectList(Enum.GetValues(typeof(EmployeePosition)), employee.Position);
                return View(employee);
            }

            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id)) return RedirectToAction("Error", "Home");
                else return RedirectToAction("Error", "Home");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            try
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
                if (employee == null) return RedirectToAction("Error", "Home");

                return View(employee);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee != null) _context.Employees.Remove(employee);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}