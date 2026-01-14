using DataCo_Website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataCo_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly DataCoTestContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(DataCoTestContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            // Hiển thị tất cả products 
            var query = _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .AsQueryable();

            var totalProducts = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            return View(products);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadActiveCategoriesAsync();
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)] // 10MB limit
        public async Task<IActionResult> Create([Bind("ProductName,CategoryId,ProductPrice,Cost,Description,Stock")] Product product, IFormFile? imageFile)
        {
            ModelState.Remove("Description");

            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    product.Image = uniqueFileName;
                }

                // set cost value
                if (!product.Cost.HasValue && product.ProductPrice.HasValue)
                    product.Cost = product.ProductPrice * 0.5;

                product.IsActive = true;
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Tạo sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadActiveCategoriesAsync(product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            await LoadActiveCategoriesAsync(product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,CategoryId,ProductPrice,Cost,Description,IsActive,Stock")] Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId)
                return NotFound();

            ModelState.Remove("Description");
            ModelState.Remove("Image");
            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .AsTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null)
                        return NotFound();

                    // Cập nhật thông tin cơ bản
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ProductPrice = product.ProductPrice;
                    existingProduct.Cost = product.Cost;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.Description = product.Description;
                    existingProduct.Stock = product.Stock; 

                    // Xử lý upload hình ảnh mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingProduct.Image))
                        {
                            string oldFilePath = Path.Combine(uploadsFolder, existingProduct.Image);
                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);
                        }

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingProduct.Image = uniqueFileName;
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "✅ Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                        return NotFound();
                    else
                        throw;
                }
            }

            await LoadActiveCategoriesAsync(product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                TempData["Error"] = "❌ Không tìm thấy sản phẩm!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Set IsActive = false
                product.IsActive = false;
                _context.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã vô hiệu hóa sản phẩm '{product.ProductName}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "❌ Không thể vô hiệu hóa sản phẩm!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Products/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                TempData["Error"] = "❌ Không tìm thấy sản phẩm!";
                return RedirectToAction(nameof(Index));
            }


            try
            {
                product.IsActive = true;
                _context.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã kích hoạt lại sản phẩm '{product.ProductName}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "❌ Không thể kích hoạt lại sản phẩm!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method 
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int stock)
        {
            if (stock < 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ" });
            }

            try
            {
                var product = await _context.Products
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                // Cập nhật Stock
                product.Stock = stock;

                // Đánh dấu entity đã modified 
                _context.Entry(product).State = EntityState.Modified;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật tồn kho thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
        private async Task LoadActiveCategoriesAsync(byte? selectedCategoryId = null)
        {
            var categories = await _context.Categories
                .Include(c => c.Department)
                .Where(c =>
                    (c.IsActive && c.Department != null && c.Department.IsActive)
                    || c.CategoryId == selectedCategoryId
                )
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(
                categories,
                "CategoryId",
                "CategoryName",
                selectedCategoryId
            );
        }

    }
}