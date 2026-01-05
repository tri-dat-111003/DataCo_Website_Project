using DataCo_Website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataCo_Website.Controllers
{
    /// <summary>
    /// Controller quản lý Đơn hàng (Orders)
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DataCoTestContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(DataCoTestContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== GET: Order/OrderConfirmation ====================
        /// <summary>
        /// Xác nhận đơn hàng sau khi checkout thành công
        /// </summary>
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .Include(o => o.Shipping)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == user.CustomerId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==================== GET: Order/MyOrders ====================
        /// <summary>
        /// Xem lịch sử đơn hàng của khách hàng
        /// </summary>
        public async Task<IActionResult> MyOrders(int page = 1)
        {
            const int pageSize = 10;

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
                return RedirectToAction("Login", "Account");

            // Tổng số order của user
            var totalOrders = await _context.Orders
                .Where(o => o.CustomerId == user.CustomerId)
                .CountAsync();

            var orders = await _context.Orders
                .Where(o => o.CustomerId == user.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Shipping)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            return View(orders);
        }

        // ==================== GET: Order/OrderDetails/5 ====================
        /// <summary>
        /// Chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Department)
                .Include(o => o.Shipping)
                .FirstOrDefaultAsync(m => m.OrderId == id && m.CustomerId == user.CustomerId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==================== POST: Order/ConfirmDelivery ====================
        /// <summary>
        /// Khách hàng xác nhận đã nhận hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var order = await _context.Orders
                .Include(o => o.Shipping)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == user.CustomerId);

            if (order == null || order.Shipping == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            if (order.Shipping.DeliveryStatus != "Waiting for Shipment")
            {
                return Json(new { success = false, message = "Đơn hàng đã được xử lý rồi" });
            }

            var shipping = order.Shipping;
            var deliveryDate = DateTime.Now;

            // Cập nhật ShippingDate = ngày nhận hàng thực tế
            shipping.ShippingDate = deliveryDate;

            // Tính số ngày giao hàng thực tế
            if (shipping.OrderDate.HasValue)
            {
                shipping.DaysForShippingActual = (int)(deliveryDate - shipping.OrderDate.Value).TotalDays;

                // Xác định DeliveryStatus
                if (shipping.DaysForShippingActual > shipping.DaysForShipmentScheduled)
                {
                    shipping.DeliveryStatus = "Late delivery";
                    shipping.LateDeliveryRisk = true;
                }
                else if (shipping.DaysForShippingActual == shipping.DaysForShipmentScheduled)
                {
                    shipping.DeliveryStatus = "Shipping on time";
                    shipping.LateDeliveryRisk = false;
                }
                else
                {
                    shipping.DeliveryStatus = "Advance shipping";
                    shipping.LateDeliveryRisk = false;
                }
            }

            // Cập nhật status order
            order.Status = "COMPLETE";

            _context.Update(shipping);
            _context.Update(order);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đã xác nhận nhận hàng",
                deliveryStatus = shipping.DeliveryStatus
            });
        }

        // ==================== POST: Order/CancelOrder ====================
        /// <summary>
        /// Khách hàng hủy đơn hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var order = await _context.Orders
                .Include(o => o.Shipping)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == user.CustomerId);

            if (order == null || order.Shipping == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép hủy đơn đang xử lý hoặc chờ giao hàng
            if (order.Status == "COMPLETE" || order.Status == "CANCELED")
            {
                return Json(new { success = false, message = "Không thể hủy đơn hàng này" });
            }

            var shipping = order.Shipping;
            var cancelDate = DateTime.Now;

            // Cập nhật thông tin shipping
            shipping.ShippingDate = cancelDate;
            shipping.LateDeliveryRisk = false;
            shipping.DaysForShipmentScheduled = 0;
            shipping.DaysForShippingActual = 0;
            shipping.DeliveryStatus = "Cancel delivery";

            // Cập nhật status order
            order.Status = "CANCELED";

            _context.Update(shipping);
            _context.Update(order);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đã hủy đơn hàng thành công"
            });
        }
    }
}
