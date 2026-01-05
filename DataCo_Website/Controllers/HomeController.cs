using DataCo_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DataCo_Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataCoTestContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DataCoTestContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ⭐ INDEX - Trang chủ với banner và departments
        public async Task<IActionResult> Index()
        {
            // Lấy departments đang active với categories và products
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .Include(d => d.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Products.Where(p => p.IsActive))
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // Lấy 8 sản phẩm mới nhất (Featured Products)
            var featuredProducts = await _context.Products
                .Where(p => p.IsActive && p.Category.IsActive && p.Category.Department.IsActive)
                .Include(p => p.Category)
                    .ThenInclude(c => c.Department)
                .OrderByDescending(p => p.ProductId)
                .Take(8)
                .ToListAsync();

            ViewBag.FeaturedProducts = featuredProducts;

            return View(departments);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}