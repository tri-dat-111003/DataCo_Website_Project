// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using DataCo_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataCo_Website.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;
        private readonly DataCoTestContext _context;

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            DataCoTestContext context,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // Thuộc tính để hiển thị ra view
        public ApplicationUser CurrentUser { get; set; } = null!;
        public Customer? CustomerInfo { get; set; }
        // SelectList for initial Country dropdown (states/cities/zips fetched via AJAX)
        public SelectList Countries { get; set; } = new SelectList(new List<string>());

        [BindProperty]
        public InputModel CustomerInput { get; set; } = new InputModel();

        public class InputModel
        {
            [Required, StringLength(50)]
            public string? FirstName { get; set; }

            [Required, StringLength(50)]
            public string? LastName { get; set; }

            // These are the chosen values from dropdowns
            [Required]
            public string? Country { get; set; }

            [Required]
            public string? State { get; set; }

            [Required]
            public string? City { get; set; }

            [Required]
            public string? Zipcode { get; set; }

            public string? Segment { get; set; } // readonly display
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            CurrentUser = user;
            CustomerInfo = await _context.Customers.FindAsync(user.CustomerId);

            if (CustomerInfo == null) return NotFound();

            // load location for prefill
            var loc = await _context.Locations.FirstOrDefaultAsync(l => l.Zipcode == CustomerInfo.Zipcode);

            CustomerInput = new InputModel
            {
                FirstName = CustomerInfo.FirstName,
                LastName = CustomerInfo.LastName,
                Country = loc?.Country,
                State = loc?.State,
                City = loc?.City,
                Zipcode = CustomerInfo.Zipcode,
                Segment = CustomerInfo.Segment
            };

            // Load list of countries for initial dropdown
            var countries = await _context.Locations.Select(l => l.Country).Distinct().OrderBy(c => c).ToListAsync();
            Countries = new SelectList(countries, CustomerInput.Country);

            return Page();
        }
        public async Task<IActionResult> OnPostUpdateCustomerInfoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = await _context.Customers.FindAsync(user.CustomerId);
            if (customer == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // reload countries for redisplay
                var countries = await _context.Locations.Select(l => l.Country).Distinct().OrderBy(c => c).ToListAsync();
                Countries = new SelectList(countries, CustomerInput.Country);
                return Page();
            }

            // Validate the selected combo exists in Locations table
            var locationExists = await _context.Locations.AnyAsync(l =>
                l.Country == CustomerInput.Country &&
                l.State == CustomerInput.State &&
                l.City == CustomerInput.City &&
                l.Zipcode == CustomerInput.Zipcode);

            if (!locationExists)
            {
                ModelState.AddModelError(string.Empty, "The selected location combination is invalid. Please pick values from the dropdowns.");
                var countries = await _context.Locations.Select(l => l.Country).Distinct().OrderBy(c => c).ToListAsync();
                Countries = new SelectList(countries, CustomerInput.Country);
                return Page();
            }

            // Update allowed fields
            customer.FirstName = CustomerInput.FirstName;
            customer.LastName = CustomerInput.LastName;
            customer.Zipcode = CustomerInput.Zipcode; // store zip only
            // do not change Segment here

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Customer info updated.";
            return RedirectToPage();
        }
    }
}
