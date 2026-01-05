// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DataCo_Website.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace DataCo_Website.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        
        //thêm:
        private readonly DataCoTestContext _context;
        //end thêm

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,

            //thêm:
            DataCoTestContext context)
            //end thêm
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;

            //thêm: 
            _context = context;
            //end thêm
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        //thêm:
        // SelectLists for dropdowns
        public SelectList Countries { get; set; }
        public SelectList States { get; set; }
        public SelectList Cities { get; set; }
        public SelectList Zipcodes { get; set; }
        public SelectList Segments { get; set; }
        //end thêm
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            //thêm:
            [Required]
            [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Country")]
            public string Country { get; set; }

            [Required]
            [Display(Name = "State")]
            public string State { get; set; }

            [Required]
            [Display(Name = "City")]
            public string City { get; set; }

            [Required]
            [Display(Name = "Zipcode")]
            public string ZipcodeId { get; set; }

            [Required]
            [Display(Name = "Segment")]
            public string Segment { get; set; }
            //end thêm
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            //thêm:
            // Load initial data for dropdowns
            await LoadDropdownData();
            //end thêm
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //thêm:
                // Verify ZipcodeId exists and matches Country, State, City
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l =>
                        l.Zipcode == Input.ZipcodeId &&
                        l.Country == Input.Country &&
                        l.State == Input.State &&
                        l.City == Input.City);

                if (location == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid location selection.");
                    await LoadDropdownData();
                    return Page();
                }
                //end thêm

                //thêm:                           
                // 1. Tạo Customer mới
                var customer = new Customer
                {
                    //CustomerId = newCustomerId,
                    Segment = Input.Segment,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Zipcode = Input.ZipcodeId
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                //end thêm

                var user = CreateUser();

                //thêm:
                //user.CustomerId = newCustomerId;   // gán FK custom
                user.CustomerId = customer.CustomerId;
                //end thêm

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    
                    

                    //thêm:
                    // Create Customer record
                    try
                    {
                        customer.CustomerId = user.CustomerId.Value;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                        //gán role mặc định user:
                        await _userManager.AddToRoleAsync(user, "User");                        

                        _logger.LogInformation("Customer record created for user {UserId}", user.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating customer record for user {UserId}", user.Id);

                        // Rollback user creation
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError(string.Empty, "Error creating customer profile. Please try again.");
                        await LoadDropdownData();
                        return Page();
                    }
                    //end thêm

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            //thêm
            // If we got this far, something failed, redisplay form
            await LoadDropdownData();
            //end thêm

            return Page();
        }

        //thêm:
        private async Task LoadDropdownData(string selectedCountry = null, string selectedState = null, string selectedCity = null)
        {
            // Load Countries
            Countries = new SelectList(
                await _context.Locations
                    .Select(l => l.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),
                selectedCountry);

            // Load Segments
            Segments = new SelectList(new List<string>
            {
                "Corporate",
                "Home Office",
                "Consumer"
            }, selectedValue: Input?.Segment);

            // Load States if country selected
            if (!string.IsNullOrEmpty(selectedCountry))
            {
                States = new SelectList(
                    await _context.Locations
                        .Where(l => l.Country == selectedCountry)
                        .Select(l => l.State)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToListAsync(),
                    selectedState);
            }

            // Load Cities if state selected
            if (!string.IsNullOrEmpty(selectedCountry) && !string.IsNullOrEmpty(selectedState))
            {
                Cities = new SelectList(
                    await _context.Locations
                        .Where(l => l.Country == selectedCountry && l.State == selectedState)
                        .Select(l => l.City)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync(),
                    selectedCity);
            }

            // Load Zipcodes if all location fields selected
            if (!string.IsNullOrEmpty(selectedCountry) &&
                !string.IsNullOrEmpty(selectedState) &&
                !string.IsNullOrEmpty(selectedCity))
            {
                Zipcodes = new SelectList(
                await _context.Locations
                    .Where(l => l.Country == selectedCountry &&
                               l.State == selectedState &&
                               l.City == selectedCity)
                    .Select(l => new { ZipcodeId = l.Zipcode, l.Zipcode }) // map Zipcode làm cả Id và Text
                    .OrderBy(z => z.Zipcode)
                    .ToListAsync(),
                "ZipcodeId",
                "Zipcode",
                Input?.ZipcodeId);
            }
        }
        //end thêm

        //thêm:
        // AJAX endpoints for cascading dropdowns
        public async Task<JsonResult> OnGetStatesAsync(string country)
        {
            var states = await _context.Locations
                .Where(l => l.Country == country)
                .Select(l => l.State)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return new JsonResult(states);
        }

        public async Task<JsonResult> OnGetCitiesAsync(string country, string state)
        {
            var cities = await _context.Locations
                .Where(l => l.Country == country && l.State == state)
                .Select(l => l.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return new JsonResult(cities);
        }

        public async Task<JsonResult> OnGetZipcodesAsync(string country, string state, string city)
        {
            var zipcodes = await _context.Locations
                .Where(l => l.Country == country && l.State == state && l.City == city)
                .Select(l => new { value = l.Zipcode, text = l.Zipcode })
                .OrderBy(z => z.text)
                .ToListAsync();

            return new JsonResult(zipcodes);
        }

        public async Task<JsonResult> OnGetLocationByZipcodeAsync(string zipcodeId)
        {
            var location = await _context.Locations
                .Where(l => l.Zipcode == zipcodeId)
                .Select(l => new
                {
                    country = l.Country,
                    state = l.State,
                    city = l.City,
                    zipcode = l.Zipcode
                })
                .FirstOrDefaultAsync();

            return new JsonResult(location);
        }
        //end thêm
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
