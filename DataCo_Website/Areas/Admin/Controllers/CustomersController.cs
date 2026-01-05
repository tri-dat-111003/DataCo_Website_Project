using DataCo_Website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataCo_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly DataCoTestContext _context;

        public CustomersController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Admin/Customers
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            // Tổng số khách hàng
            var totalCustomers = await _context.Customers.CountAsync();

            // Lấy danh sách theo trang
            var customers = await _context.Customers
                .Include(c => c.ZipcodeNavigation)
                .OrderBy(c => c.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            // Truyền thông tin phân trang qua ViewBag
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCustomers / (double)pageSize);

            return View(customers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.ZipcodeNavigation)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

       

        // GET: Admin/Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            ViewData["Zipcode"] = new SelectList(
                _context.Locations.Select(l => new
                {
                    l.Zipcode,
                    DisplayText = $"{l.City}, {l.State}, {l.Country} ({l.Zipcode})"
                }),
                "Zipcode",
                "DisplayText",
                customer.Zipcode
            );
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Zipcode,Segment,CustomerId,FirstName,LastName")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Cập nhật khách hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Zipcode"] = new SelectList(
                _context.Locations.Select(l => new
                {
                    l.Zipcode,
                    DisplayText = $"{l.City}, {l.State}, {l.Country} ({l.Zipcode})"
                }),
                "Zipcode",
                "DisplayText",
                customer.Zipcode
            );
            return View(customer);
        }

        // GET: Admin/Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.ZipcodeNavigation)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
