using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace Employee_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] // restricts whole controller to Admins only
    public class ShiftController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shift
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Shifts.ToListAsync());
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Shift/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return RedirectToAction("Error", "Home");

                var shift = await _context.Shifts
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (shift == null) return RedirectToAction("Error", "Home");

                return View(shift);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Shift/Create
        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Shift/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,StartTime,EndTime,BreakTime")] Shift shift)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(shift);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(shift);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Shift/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return RedirectToAction("Error", "Home");

                var shift = await _context.Shifts.FindAsync(id);
                if (shift == null) return RedirectToAction("Error", "Home");

                return View(shift);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Shift/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartTime,EndTime,BreakTime")] Shift shift)
        {
            try
            {
                if (id != shift.Id) return RedirectToAction("Error", "Home");

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(shift);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ShiftExists(shift.Id))
                            return RedirectToAction("Error", "Home");
                        else
                            throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(shift);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Shift/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return RedirectToAction("Error", "Home");

                var shift = await _context.Shifts.FirstOrDefaultAsync(m => m.Id == id);
                if (shift == null) return RedirectToAction("Error", "Home");

                return View(shift);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Shift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var shift = await _context.Shifts.FindAsync(id);
                if (shift != null)
                {
                    _context.Shifts.Remove(shift);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.Id == id);
        }
    }
}