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
    public class OrdersController : Controller
    {
        private readonly DataCoTestContext _context;

        public OrdersController(DataCoTestContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            const int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;

            var eightYearAgo = DateTime.Now.AddYears(-8);

            // ✅ XÓA AsSplitQuery() - Dùng single query với index tốt
            var ordersQuery = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Department)
                .Where(o => o.OrderDate >= eightYearAgo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderId.ToString().Contains(searchString) ||
                    (o.Customer != null &&
                     (o.Customer.FirstName + " " + o.Customer.LastName).Contains(searchString)));
            }

            ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);

            var totalOrders = await ordersQuery.CountAsync();

            var orders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ✅ XÓA AsSplitQuery() - Details page không cần vì chỉ load 1 order
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                    .ThenInclude(c => c.ZipcodeNavigation)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Department)
                .Include(o => o.Shipping)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/EditStatus/5
        public async Task<IActionResult> EditStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Department)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.StatusList = new SelectList(new[]
            {
                "PENDING",
                "PROCESSING",
                "COMPLETE",
                "CLOSED",
                "CANCELED",
                "PAYMENT_REVIEW",
                "PENDING_PAYMENT",
                "SUSPECTED_FRAUD"
            }, order.Status);

            return View(order);
        }

        // POST: Admin/Orders/EditStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // CHỈ CẬP NHẬT STATUS
            order.Status = status;

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"✅ Đã cập nhật trạng thái đơn hàng #{id} thành '{status}'";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }

        // API: Quick Update Status (AJAX)
        [HttpPost]
        public async Task<IActionResult> QuickUpdateStatus(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                order.Status = status;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Đã cập nhật trạng thái thành '{status}'"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}