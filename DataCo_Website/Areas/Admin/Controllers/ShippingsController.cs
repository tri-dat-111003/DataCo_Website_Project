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
    public class ShippingsController : Controller
    {
        private readonly DataCoTestContext _context;

        public ShippingsController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Admin/Shippings (READ ONLY)
        public async Task<IActionResult> Index(string searchString, string deliveryStatus, bool? lateRisk, int page = 1)
        {
            const int pageSize = 10;

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = deliveryStatus;
            ViewData["CurrentLateRisk"] = lateRisk;

            var oneYearAgo = DateTime.Now.AddYears(-8);

            // ✅ XÓA AsSplitQuery() - Dùng single query
            var shippingsQuery = _context.Shippings
                .AsNoTracking()
                .Include(s => s.Order)
                    .ThenInclude(o => o.Customer)
                .Include(s => s.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Department)
                .Where(s => s.OrderDate >= oneYearAgo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                shippingsQuery = shippingsQuery.Where(s =>
                    s.OrderId.ToString().Contains(searchString) ||
                    (s.Order.Customer != null &&
                     (s.Order.Customer.FirstName + " " + s.Order.Customer.LastName).Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(deliveryStatus))
            {
                shippingsQuery = shippingsQuery.Where(s => s.DeliveryStatus == deliveryStatus);
            }

            if (lateRisk.HasValue)
            {
                shippingsQuery = shippingsQuery.Where(s => s.LateDeliveryRisk == lateRisk.Value);
            }

            shippingsQuery = shippingsQuery.OrderByDescending(s => s.OrderDate);

            var totalShippings = await shippingsQuery.CountAsync();

            var shippings = await shippingsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.DeliveryStatuses = await _context.Shippings
                .Where(s => s.OrderDate >= oneYearAgo && !string.IsNullOrEmpty(s.DeliveryStatus))
                .Select(s => s.DeliveryStatus)
                .Distinct()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalShippings / (double)pageSize);

            return View(shippings);
        }


        // GET: Admin/Shippings/Details/5 (READ ONLY)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ✅ XÓA AsSplitQuery()
            var shipping = await _context.Shippings
                .AsNoTracking()
                .Include(s => s.Order)
                    .ThenInclude(o => o.Customer)
                        .ThenInclude(c => c.ZipcodeNavigation)
                .Include(s => s.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Category)
                .Include(s => s.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Department)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (shipping == null)
            {
                return NotFound();
            }

            return View(shipping);
        }
    }
}
