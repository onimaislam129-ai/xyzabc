using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Client with pagination and search
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10; // 10 rows per page

            try
            {
                // Explicitly declare IQueryable to avoid CS0266
                IQueryable<Client> clientsQuery = _context.Clients
                    .OrderBy(c => c.Name);

                // Apply search filter if searchTerm is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    clientsQuery = clientsQuery.Where(c =>
                        c.Name.Contains(searchTerm) ||
                        (c.Address != null && c.Address.Contains(searchTerm)) ||
                        (c.Phone != null && c.Phone.Contains(searchTerm))
                    );
                }

                var totalRecords = await clientsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var clients = await clientsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["SearchTerm"] = searchTerm; // preserve search term in view

                return View(clients);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return RedirectToAction("Error", "Home");

            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Phone,Email,Type")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return RedirectToAction("Error", "Home");

            return View(client);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone,Email,Type")] Client client)
        {
            if (id != client.Id) return RedirectToAction("Error", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id)) return RedirectToAction("Error", "Home");
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Client/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return RedirectToAction("Error", "Home");

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return RedirectToAction("Error", "Home");

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}