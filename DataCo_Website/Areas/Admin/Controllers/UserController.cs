using DataCo_Website.Areas.Admin.Models;
using DataCo_Website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataCo_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataCoTestContext _context;

        public UserController(UserManager<ApplicationUser> userManager, DataCoTestContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Admin/User
        // ============================================
        // PHẦN 1: CONTROLLER - TỐI ƯU DATABASE
        // ============================================

        public async Task<IActionResult> Index(int page = 1, string searchEmail = "")
        {
            const int pageSize = 10;

            var baseQuery = _context.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(searchEmail))
            {
                baseQuery = baseQuery.Where(u => u.Email.Contains(searchEmail));
            }

            // ✅ CHẠY TUẦN TỰ - Không dùng Task.WhenAll với cùng 1 DbContext
            var totalUsers = await baseQuery.CountAsync();

            var users = await baseQuery
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListViewModel
                {
                    UserId = u.Id,
                    CustomerId = u.CustomerId,
                    UserName = u.UserName,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneNumber = u.PhoneNumber,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    LockoutEnd = u.LockoutEnd,
                    LockoutEnabled = u.LockoutEnabled,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewBag.SearchEmail = searchEmail;

            return View(users);
        }

        // GET: Admin/User/ChangePassword/5
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new ChangePasswordViewModel { UserId = user.Id, Email = user.Email });
        }

        // POST: Admin/User/ChangePassword/5
        // Logic: Đổi pass -> Mở khóa tài khoản (Reset Lockout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            // 1. Xóa password cũ
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // 2. Thêm password mới
            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (addResult.Succeeded)
            {
                // 3. QUAN TRỌNG: Reset Lockout để user đăng nhập được ngay
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _userManager.SetLockoutEndDateAsync(user, null); // Xóa thời gian khóa
                    await _userManager.ResetAccessFailedCountAsync(user);  // Reset số lần sai về 0
                }

                // Đảm bảo LockoutEnabled = true để tính năng bảo mật vẫn chạy
                user.LockoutEnabled = true;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "✅ Đổi mật khẩu thành công & Đã mở khóa tài khoản!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in addResult.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new EditUserViewModel
            {
                UserId = user.Id,
                CustomerId = user.CustomerId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled
                // Bỏ IsActive - dùng Delete/Restore
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.UserId) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email; // Thường UserName = Email
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
            user.TwoFactorEnabled = model.TwoFactorEnabled;
            user.LockoutEnabled = model.LockoutEnabled;
            // Không cập nhật IsActive - dùng Delete/Restore

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "✅ Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Chỉ lấy thông tin cơ bản để hiển thị xác nhận
            var user = await _userManager.Users.AsNoTracking()
                .Select(u => new { u.Id, u.UserName, u.Email, u.CustomerId, u.IsActive })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Map tạm vào User model để view dùng lại được (hoặc tạo ViewModel riêng nếu muốn chặt chẽ)
            var model = new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CustomerId = user.CustomerId,
                IsActive = user.IsActive
            };

            return View(model);
        }

        // POST: Admin/User/Delete/5
        // Logic: Tắt IsActive (User) -> Tắt IsActive (Customer)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 1. Tắt IsActive của User
            user.IsActive = false;
            user.LockoutEnd = DateTimeOffset.MaxValue; // Khóa luôn cho chắc (tuỳ chọn)
            var result = await _userManager.UpdateAsync(user);

            // 2. Tắt IsActive của Customer (dựa vào CustomerId)
            if (result.Succeeded && user.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);
                if (customer != null)
                {
                    customer.IsActive = false;
                    _context.Customers.Update(customer);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "✅ Đã vô hiệu hóa User và Customer liên quan!";
            return RedirectToAction(nameof(Index));
        }

        // Khôi phục (nếu cần dùng nút Restore)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 1. Bật lại User
            user.IsActive = true;
            user.LockoutEnd = null; // Mở khóa
            await _userManager.UpdateAsync(user);

            // 2. Bật lại Customer
            if (user.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);
                if (customer != null)
                {
                    customer.IsActive = true;
                    _context.Customers.Update(customer);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "✅ Đã khôi phục User và Customer!";
            return RedirectToAction(nameof(Index));
        }

        /*[HttpGet]
        public async Task<IActionResult> GetUsersPartial(int page = 1, string searchEmail = "")
        {
            const int pageSize = 20;

            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(searchEmail))
            {
                query = query.Where(u => u.Email.Contains(searchEmail));
            }

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListViewModel
                {
                    UserId = u.Id,
                    CustomerId = u.CustomerId,
                    UserName = u.UserName,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneNumber = u.PhoneNumber,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    LockoutEnd = u.LockoutEnd,
                    LockoutEnabled = u.LockoutEnabled,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return PartialView("_UserTableRows", users);
        }*/
    }
}