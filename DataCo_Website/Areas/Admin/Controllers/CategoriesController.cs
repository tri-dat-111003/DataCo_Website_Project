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
    public class CategoriesController : Controller
    {
        private readonly DataCoTestContext _context;

        public CategoriesController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var query = _context.Categories
                .Include(c => c.Products)
                .Include(c => c.Department)
                .AsQueryable();

            var totalCategories = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.CategoryId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);

            return View(categories);
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(byte? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            await LoadActiveDepartmentsAsync();
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,DepartmentId")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.IsActive = true;
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Tạo danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadActiveDepartmentsAsync(category.DepartmentId);
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(byte? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            await LoadActiveDepartmentsAsync(category.DepartmentId);
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(byte id, [Bind("CategoryId,CategoryName,DepartmentId,IsActive")] Category category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await LoadActiveDepartmentsAsync(category.DepartmentId);
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(byte? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive))
                .Include(c => c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null)
                return NotFound();

            ViewBag.ActiveProductCount = category.Products.Count;
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(byte id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "❌ Không tìm thấy danh mục!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // XÓA MỀM: Set IsActive = false
                category.IsActive = false;
                _context.Update(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã vô hiệu hóa danh mục '{category.CategoryName}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "❌ Không thể vô hiệu hóa danh mục!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Categories/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(byte id)
        {
            var category = await _context.Categories
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "❌ Không tìm thấy danh mục!";
                return RedirectToAction(nameof(Index));
            }

            

            try
            {
                category.IsActive = true;
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"✅ Đã kích hoạt lại danh mục '{category.CategoryName}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "❌ Không thể kích hoạt lại danh mục!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(byte id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        // ✅ Helper method để load Department dropdown
        private async Task LoadActiveDepartmentsAsync(int? selectedDepartmentId = null)
        {
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            ViewBag.Departments = new SelectList(
                departments,
                "DepartmentId",
                "DepartmentName",
                selectedDepartmentId
            );
        }
    }
}
