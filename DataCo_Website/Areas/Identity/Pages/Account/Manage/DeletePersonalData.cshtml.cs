// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DataCo_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DataCo_Website.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly DataCoTestContext _context;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            DataCoTestContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mật khẩu để xác nhận")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);

            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
                    return Page();
                }
            }

            // ✅ SOFT DELETE: Tắt IsActive thay vì xóa hẳn
            try
            {
                // 1. Tắt IsActive của User
                user.IsActive = false;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Khóa vĩnh viễn
                var updateUserResult = await _userManager.UpdateAsync(user);

                if (!updateUserResult.Succeeded)
                {
                    throw new InvalidOperationException($"Lỗi khi vô hiệu hóa tài khoản User.");
                }

                // 2. Tắt IsActive của Customer (nếu có liên kết)
                if (user.CustomerId.HasValue)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);

                    if (customer != null)
                    {
                        customer.IsActive = false;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                    }
                }

                // 3. Log thông tin
                _logger.LogInformation("User with ID '{UserId}' deactivated their account (soft delete).", user.Id);

                // 4. Đăng xuất ngay lập tức
                await _signInManager.SignOutAsync();

                // 5. Chuyển về trang chủ với thông báo
                TempData["AccountDeleted"] = "Tài khoản của bạn đã được vô hiệu hóa. Liên hệ admin nếu muốn khôi phục.";
                return Redirect("~/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating user account.");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi vô hiệu hóa tài khoản.");
                return Page();
            }
        }
    }
}