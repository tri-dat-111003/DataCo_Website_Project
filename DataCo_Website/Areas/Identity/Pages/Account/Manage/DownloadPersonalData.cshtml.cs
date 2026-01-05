// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DataCo_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCo_Website.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataCoTestContext _context;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            DataCoTestContext context,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var customer = await _context.Customers.FindAsync(user.CustomerId);

            // Nếu bạn dùng IdentityUser mặc định, bạn có thể lấy thêm thông tin như sau:
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

            var exportData = new
            {
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.EmailConfirmed,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    AuthenticatorKey = authenticatorKey
                },
                Customer = customer != null ? new
                {
                    customer.CustomerId,
                    customer.FirstName,
                    customer.LastName,
                    customer.Zipcode,
                    customer.Segment
                } : null
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            _logger.LogInformation("User {UserId} downloaded their personal data.", user.Id);

            return File(bytes, "application/json", "PersonalData.json");
        }

    }
}
