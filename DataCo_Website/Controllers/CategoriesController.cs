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
    public class CategoriesController : Controller
    {
        private readonly DataCoTestContext _context;

        public CategoriesController(DataCoTestContext context)
        {
            _context = context;
        }

        // ⭐ INDEX - Hiển thị tất cả categories theo department
        public async Task<IActionResult> Index()
        {
            // Lấy departments với categories để hiển thị đẹp
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .Include(d => d.Categories.Where(c => c.IsActive))
                    .ThenInclude(c => c.Products.Where(p => p.IsActive))
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return View(departments);
        }
    }
}
