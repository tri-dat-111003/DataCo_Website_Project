using DataCo_Website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCo_Website.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly DataCoTestContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(DataCoTestContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== GET: Cart/Index ====================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await GetOrCreateCartAsync(user.CustomerId.Value);

            return View(cart);
        }

        // ==================== POST: Cart/AddToCart ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Kiểm tra product active
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (!product.IsActive)
            {
                return Json(new { success = false, message = "Sản phẩm hiện không còn khả dụng" });
            }

            // Kiểm tra Category và Department active
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.CategoryId == product.CategoryId);

            if (category == null || !category.IsActive || category.Department == null || !category.Department.IsActive)
            {
                return Json(new { success = false, message = "Sản phẩm hiện không còn khả dụng" });
            }

            // ⭐ QUAN TRỌNG: Query cart RIÊNG để tránh tracking conflict
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = user.CustomerId.Value,
                    CreatedDate = DateTime.Now,
                    CurrentSessionId = GenerateSessionId()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Tạo session nếu chưa có VÀ gán session mới cho các items cũ
            if (string.IsNullOrEmpty(cart.CurrentSessionId))
            {
                cart.CurrentSessionId = GenerateSessionId();
                _context.Entry(cart).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // ⭐ THÊM MỚI: Gán session mới cho items cũ có SessionId = null
                // Đây là items còn lại sau checkout trước đó
                var orphanItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId
                              && ci.Status != "CheckedOut"
                              && string.IsNullOrEmpty(ci.SessionId))
                    .ToListAsync();

                if (orphanItems.Any())
                {
                    foreach (var item in orphanItems)
                    {
                        item.SessionId = cart.CurrentSessionId; // Gán session mới
                        item.UpdatedDate = DateTime.Now;
                        _context.Entry(item).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                }
            }

            // ⭐ QUAN TRỌNG: Query CartItem RIÊNG để tránh tracking conflict
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.CartId
                    && ci.ProductId == productId
                    && ci.SessionId == cart.CurrentSessionId
                    && ci.Status != "CheckedOut");

            if (existingItem != null)
            {
                // CỘNG DỒN số lượng
                existingItem.Quantity += quantity;
                existingItem.UpdatedDate = DateTime.Now;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
            else
            {
                // Tạo mới
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    Status = "InCart",
                    SessionId = cart.CurrentSessionId,
                    AddedDate = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedDate = DateTime.Now;
            _context.Entry(cart).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            // ⭐ Trả về số loại sản phẩm (không phải tổng số lượng)
            var itemCount = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId
                    && ci.SessionId == cart.CurrentSessionId
                    && ci.Status != "CheckedOut")
                .CountAsync();

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartCount = itemCount });
        }

        // ==================== POST: Cart/ToggleSelect ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSelect(int cartItemId, bool isSelected)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId
                    && ci.Cart.CustomerId == user.CustomerId.Value);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            // ⭐ QUAN TRỌNG: Đổi status và đánh dấu Modified
            cartItem.Status = isSelected ? "Selected" : "InCart";
            cartItem.UpdatedDate = DateTime.Now;
            _context.Entry(cartItem).State = EntityState.Modified;

            cartItem.Cart.UpdatedDate = DateTime.Now;
            _context.Entry(cartItem.Cart).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ==================== POST: Cart/UpdateQuantity ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId
                    && ci.Cart.CustomerId == user.CustomerId.Value
                    && ci.Status != "CheckedOut");

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // ⭐ QUAN TRỌNG: Update và đánh dấu Modified
                cartItem.Quantity = quantity;
                cartItem.UpdatedDate = DateTime.Now;
                _context.Entry(cartItem).State = EntityState.Modified;
            }

            cartItem.Cart.UpdatedDate = DateTime.Now;
            _context.Entry(cartItem.Cart).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ==================== POST: Cart/RemoveItem ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId
                    && ci.Cart.CustomerId == user.CustomerId.Value
                    && ci.Status != "CheckedOut");

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            _context.CartItems.Remove(cartItem);

            cartItem.Cart.UpdatedDate = DateTime.Now;
            _context.Entry(cartItem.Cart).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ==================== GET: Cart/Checkout ====================
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await GetOrCreateCartAsync(user.CustomerId.Value);

            var selectedItems = cart.CartItems
                .Where(ci => ci.Status == "Selected"
                          && ci.SessionId == cart.CurrentSessionId)
                .ToList();

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán";
                return RedirectToAction("Index");
            }

            var hasInactiveProducts = selectedItems.Any(ci =>
                !ci.Product.IsActive ||
                !ci.Product.Category.IsActive ||
                !ci.Product.Category.Department.IsActive);

            if (hasInactiveProducts)
            {
                TempData["Warning"] = "Một số sản phẩm không còn khả dụng. Vui lòng bỏ chọn hoặc xóa.";
            }

            await _context.Entry(cart).Reference(c => c.Customer).LoadAsync();
            await _context.Entry(cart.Customer).Reference(c => c.ApplicationUser).LoadAsync();

            return View(cart);
        }

        // ==================== POST: Cart/ProcessCheckout ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(
        string shippingAddress,
        string paymentMethod,
        string orderRegion,
        string orderCountry,
        string orderState,
        string orderCity,
        string shippingMode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // ✅ VALIDATION
            if (string.IsNullOrWhiteSpace(shippingAddress) ||
                string.IsNullOrWhiteSpace(orderCountry) ||
                string.IsNullOrWhiteSpace(shippingMode) ||
                string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin giao hàng!";
                return RedirectToAction("Checkout");
            }

            var (regionFromApi, marketFromApi) =
                await GetRegionAndMarketFromApiAsync(orderCountry);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var cart = await GetOrCreateCartAsync(user.CustomerId.Value);

                        // ✅ LẤY ITEMS ĐƯỢC CHỌN
                        var selectedItems = cart.CartItems
                            .Where(ci => ci.Status == "Selected"
                                      && ci.SessionId == cart.CurrentSessionId)
                            .ToList();

                        if (!selectedItems.Any())
                        {
                            TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm";
                            return RedirectToAction("Index");
                        }

                        // ✅ CHECK ACTIVE
                        var inactiveItems = selectedItems
                            .Where(ci => !ci.Product.IsActive
                                || !ci.Product.Category.IsActive
                                || !ci.Product.Category.Department.IsActive)
                            .ToList();

                        if (inactiveItems.Any())
                        {
                            TempData["Error"] =
                                $"Có {inactiveItems.Count} sản phẩm không còn khả dụng";
                            return RedirectToAction("Index");
                        }

                        // ✅ TẠO ORDER
                        var order = new Order
                        {
                            CustomerId = user.CustomerId.Value,
                            Status = "PROCESSING",
                            OrderDate = DateTime.Now,
                            TypeTransaction = paymentMethod,
                            OrderCity = orderCity,
                            OrderState = orderState,
                            OrderCountry = orderCountry,
                            OrderRegion = !string.IsNullOrWhiteSpace(orderRegion)
                                ? orderRegion
                                : regionFromApi,
                            Market = marketFromApi,
                            AddressLine = shippingAddress
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync(); // lấy OrderId

                        // ✅ TẠO ORDER ITEMS
                        foreach (var cartItem in selectedItems)
                        {
                            var product = cartItem.Product;
                            int departmentId =
                                product?.Category?.DepartmentId ?? 1;

                            double sales =
                                cartItem.Quantity * (product?.ProductPrice ?? 0);
                            double cost =
                                (product?.Cost ?? 0) * cartItem.Quantity;
                            double profit = sales - cost;
                            double profitRatio =
                                sales > 0 ? profit / sales : 0;

                            _context.OrderItems.Add(new OrderItem
                            {
                                OrderId = order.OrderId,
                                ProductId = cartItem.ProductId,
                                Quantity = cartItem.Quantity,
                                Discount = 0,
                                DiscountRate = 0,
                                Sales = sales,
                                Total = sales,
                                ProfitRatio = profitRatio,
                                DepartmentId = departmentId
                            });
                        }

                        await _context.SaveChangesAsync();

                        // ✅ TẠO SHIPPING
                        int daysForShipment = GetDaysForShipment(shippingMode);
                        var shipping = new Shipping
                        {
                            OrderId = order.OrderId,
                            OrderDate = order.OrderDate,
                            ShippingDate = order.OrderDate,
                            ShippingMode = shippingMode,
                            DaysForShipmentScheduled = daysForShipment,
                            DaysForShippingActual = 0,
                            LateDeliveryRisk = false,
                            DeliveryStatus = "Waiting for Shipment"
                        };

                        _context.Shippings.Add(shipping);
                        await _context.SaveChangesAsync();

                        // ✅ UPDATE CART ITEMS ĐÃ MUA
                        foreach (var cartItem in selectedItems)
                        {
                            cartItem.Status = "CheckedOut";
                            cartItem.CheckedOutDate = DateTime.Now;
                            cartItem.UpdatedDate = DateTime.Now;
                            _context.Entry(cartItem).State = EntityState.Modified;
                        }

                        // ⭐ CHỈ SAVE – KHÔNG RESET SESSION
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        TempData["Success"] = "Đặt hàng thành công!";

                        return RedirectToAction(
                            "OrderConfirmation",
                            "Order",
                            new { orderId = order.OrderId });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                        return RedirectToAction("Checkout");
                    }
                }
            });
        }


        // ==================== GET: Cart/MyOrders ====================
        public async Task<IActionResult> MyOrders(int page = 1)
        {
            const int pageSize = 10;

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
                return RedirectToAction("Login", "Account");

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

        // ==================== GET: Cart/GetCartCount ====================
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CustomerId == null)
            {
                return Json(new { count = 0 });
            }

            var cart = await _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);

            if (cart == null || string.IsNullOrEmpty(cart.CurrentSessionId))
            {
                return Json(new { count = 0 });
            }

            var count = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId
                    && ci.SessionId == cart.CurrentSessionId
                    && ci.Status != "CheckedOut")
                .CountAsync();

            return Json(new { count });
        }

        // ==================== HELPER METHODS ====================

        private async Task<Cart> GetOrCreateCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems.Where(ci => ci.Status != "CheckedOut"))
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                            .ThenInclude(c => c.Department)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private string GenerateSessionId()
        {
            return $"SESSION_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private int GetDaysForShipment(string shippingMode)
        {
            return shippingMode switch
            {
                "Standard Class" => 4,
                "Second Class" => 2,
                "Same Day" => 0,
                "First Class" => 1,
                _ => 4
            };
        }

        private async Task<(string region, string market)> GetRegionAndMarketFromApiAsync(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
                return ("Unknown", "Unknown");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync($"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(country)}");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement[0];

                    string region = root.GetProperty("region").GetString() ?? "Unknown";
                    string subregion = root.TryGetProperty("subregion", out var sr) ? sr.GetString() ?? "Unknown" : "Unknown";

                    string market = GetMarketFromRegion(region, subregion);

                    return (region, market);
                }
                catch
                {
                    return ("Unknown", "Unknown");
                }
            }
        }

        private string GetMarketFromRegion(string region, string subregion)
        {
            region = region.ToLower();
            subregion = subregion.ToLower();

            if (region.Contains("europe"))
                return "Europe";
            if (region.Contains("africa"))
                return "Africa";
            if (region.Contains("americas"))
                return subregion.Contains("south") ? "LATAM" : "USCA";
            if (region.Contains("asia") || region.Contains("oceania"))
                return "Pacific Asia";

            return "Unknown";
        }

        [HttpGet]
        public async Task<IActionResult> GetRegionMarket(string country)
        {
            var (region, market) = await GetRegionAndMarketFromApiAsync(country);
            return Json(new { region, market });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartBySlug(
        string departmentSlug,
        string categorySlug,
        string productSlug,
        int quantity = 1)
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
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            // Gọi lại AddToCart cũ - REUSE CODE
            return await AddToCart(product.ProductId, quantity);
        }
    }
}