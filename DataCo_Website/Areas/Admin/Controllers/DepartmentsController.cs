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
    public class DepartmentsController : Controller
    {
        private readonly DataCoTestContext _context;

        public DepartmentsController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Admin/Departments
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            // Hiển thị tất cả phòng ban 
            var query = _context.Departments.AsQueryable();

            var totalDepartments = await query.CountAsync();

            var departments = await query
                .OrderBy(d => d.DepartmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            // Tính OrderCount sau khi load
            foreach (var dept in departments)
            {
                dept.OrderCount = await _context.OrderItems
                    .Where(oi => oi.DepartmentId == dept.DepartmentId)
                    .Select(oi => oi.OrderId)
                    .Distinct()
                    .CountAsync();
            }

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalDepartments / (double)pageSize);

            return View(departments);
        }

        public async Task<IActionResult> Details(int? id, int page = 1)
        {
            if (id == null)
                return NotFound();

            const int pageSize = 10;

            var department = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
                return NotFound();

            // Lấy các OrderId thuộc department này
            var distinctOrderIds = await _context.OrderItems
                .Where(oi => oi.DepartmentId == id)
                .Select(oi => oi.OrderId)
                .Distinct()
                .OrderByDescending(oid => oid)
                .ToListAsync();

            var totalOrders = distinctOrderIds.Count;

            // Lấy orders theo page
            var pagedOrderIds = distinctOrderIds
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Load order details
            var ordersWithDetails = await _context.Orders
                .Where(o => pagedOrderIds.Contains(o.OrderId))
                .Include(o => o.Customer)
                .AsNoTracking()
                .ToListAsync();

            // Tính TotalAmount cho mỗi order
            var orders = new List<object>();
            foreach (var o in ordersWithDetails.OrderByDescending(o => o.OrderDate))
            {
                var totalAmount = await _context.OrderItems
                    .Where(oi => oi.OrderId == o.OrderId && oi.DepartmentId == id)
                    .SumAsync(oi => (decimal?)oi.Total) ?? 0;

                orders.Add(new
                {
                    o.OrderId,
                    CustomerName = o.Customer != null
                        ? $"{o.Customer.FirstName} {o.Customer.LastName}"
                        : "N/A",
                    o.OrderDate,
                    TotalAmount = totalAmount
                });
            }

            // Đếm số category và product thuộc department
            var categoryCount = await _context.Categories
                .Where(c => c.DepartmentId == id)
                .CountAsync();

            var productCount = await _context.Products
                .Where(p => p.Category != null && p.Category.DepartmentId == id)
                .CountAsync();

            ViewBag.Orders = orders;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            ViewBag.CategoryCount = categoryCount;
            ViewBag.ProductCount = productCount;

            return View(department);
        }

        // GET: Admin/Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentName,AddressLine")] Department department)
        {
            if (ModelState.IsValid)
            {
                department.IsActive = true; // Mặc định active
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Tạo phòng ban thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Admin/Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Admin/Departments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,DepartmentName,AddressLine")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Cập nhật phòng ban thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
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
            return View(department);
        }

        // GET: Admin/Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            // Đếm số category và product thuộc department
            var categoryCount = await _context.Categories
                .Where(c => c.DepartmentId == id && c.IsActive)
                .CountAsync();

            var productCount = await _context.Products
                .Where(p => p.Category != null && p.Category.DepartmentId == id && p.IsActive)
                .CountAsync();

            ViewBag.CategoryCount = categoryCount;
            ViewBag.ProductCount = productCount;

            return View(department);
        }

        // POST: Admin/Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                TempData["Error"] = "❌ Không tìm thấy phòng ban!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // XÓA MỀM: Chỉ set IsActive = false
                department.IsActive = false;
                _context.Update(department);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã vô hiệu hóa phòng ban '{department.DepartmentName}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "❌ Không thể vô hiệu hóa phòng ban!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Departments/Restore/5 (KHÔI PHỤC)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                TempData["Error"] = "❌ Không tìm thấy phòng ban!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                department.IsActive = true;
                _context.Update(department);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã kích hoạt lại phòng ban '{department.DepartmentName}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "❌ Không thể kích hoạt lại phòng ban!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentId == id);
        }
    }
}
