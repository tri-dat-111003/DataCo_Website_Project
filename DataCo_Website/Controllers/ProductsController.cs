using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataCo_Website.Models;

namespace DataCo_Website.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DataCoTestContext _context;

        public ProductsController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId, string searchQuery, int page = 1)
        {
            const int pageSize = 20;

            // ✅ Chỉ hiển thị product ACTIVE, category ACTIVE, department ACTIVE
            var productsQuery = _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .Where(p => p.IsActive
                    && p.Category.IsActive
                    && p.Category.Department.IsActive) // ⭐ PHÉP && (3 lớp)
                .AsQueryable();

            // Filter by category
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                ViewBag.CategoryName = await _context.Categories
                    .Where(c => c.CategoryId == categoryId.Value)
                    .Select(c => c.CategoryName)
                    .FirstOrDefaultAsync();
                ViewBag.ShowBackToCategories = true;
            }
            else
            {
                ViewBag.CategoryName = null;
                ViewBag.ShowBackToCategories = false;
            }

            // Filter by search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchQuery) ||
                    p.Category.CategoryName.Contains(searchQuery));
                ViewBag.SearchQuery = searchQuery;
            }

            var totalProducts = await productsQuery.CountAsync();
            var products = await productsQuery
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ✅ Chỉ hiển thị nếu product, category, department đều ACTIVE
            var product = await _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .Where(p => p.ProductId == id
                    && p.IsActive
                    && p.Category.IsActive
                    && p.Category.Department.IsActive) // ⭐ PHÉP &&
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> IndexBySlug(string departmentSlug, string categorySlug, int page = 1)
        {
            departmentSlug = Uri.UnescapeDataString(departmentSlug).ToLowerInvariant().Trim();
            categorySlug = Uri.UnescapeDataString(categorySlug).ToLowerInvariant().Trim();

            var category = await _context.Categories
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c =>
                    c.IsActive
                    && c.Department.IsActive
                    && c.CategoryName.ToLower().Trim() == categorySlug
                    && c.Department.DepartmentName.ToLower().Trim() == departmentSlug);

            if (category == null)
                return NotFound();

            // ✅ SỬA: Thực hiện logic giống Index
            const int pageSize = 20;

            var productsQuery = _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .Where(p => p.IsActive
                    && p.Category.IsActive
                    && p.Category.Department.IsActive
                    && p.CategoryId == category.CategoryId)
                .AsQueryable();

            var totalProducts = await productsQuery.CountAsync();
            var products = await productsQuery
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            // Set ViewBag giống Index
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewBag.CategoryId = category.CategoryId;
            ViewBag.CategoryName = category.CategoryName;
            ViewBag.ShowBackToCategories = true;
            ViewBag.SearchQuery = null;

            // ✅ Trả về View "Index" với data
            return View("Index", products);
        }

        public async Task<IActionResult> DetailsBySlug(
        string departmentSlug,
        string categorySlug,
        string productSlug)
        {
            departmentSlug = Uri.UnescapeDataString(departmentSlug).ToLowerInvariant().Trim();
            categorySlug = Uri.UnescapeDataString(categorySlug).ToLowerInvariant().Trim();
            productSlug = Uri.UnescapeDataString(productSlug).ToLowerInvariant().Trim();

            var product = await _context.Products
                .Include(p => p.Category)              // ⭐ THÊM DÒNG NÀY
                    .ThenInclude(c => c.Department)    // ⭐ THÊM DÒNG NÀY
                .FirstOrDefaultAsync(p =>
                    p.IsActive
                    && p.ProductName.ToLower().Trim() == productSlug
                    && p.Category.CategoryName.ToLower().Trim() == categorySlug
                    && p.Category.Department.DepartmentName.ToLower().Trim() == departmentSlug
                    && p.Category.IsActive
                    && p.Category.Department.IsActive);

            if (product == null)
                return NotFound();

            // Gọi lại action Details cũ - REUSE CODE
            return View("Details", product);
        }
    }
}