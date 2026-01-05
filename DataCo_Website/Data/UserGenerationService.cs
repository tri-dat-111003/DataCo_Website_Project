using DataCo_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserGenerationService
{
    private readonly DataCoTestContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Random _random = new Random();

    public UserGenerationService(DataCoTestContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Tạo user cho tất cả customer chưa có user
    public async Task<string> CreateUsersForAllCustomers()
    {
        var results = new List<string>();

        var customers = await _context.Customers
            .Where(c => c.ApplicationUser == null)
            .ToListAsync();

        foreach (var customer in customers)
        {
            try
            {
                // Tạo email (username = email)
                var email = CreateEmail(customer);

                var password = "Aa@123";

                var user = new ApplicationUser
                {
                    UserName = email,   // <-- username = email
                    Email = email,
                    EmailConfirmed = true,
                    CustomerId = customer.CustomerId
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    results.Add($"✓ Created user for {customer.FirstName} {customer.LastName} - Email/UserName: {email}");
                }
                else
                {
                    results.Add($"✗ Failed for {customer.FirstName} {customer.LastName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"✗ Error for {customer.FirstName} {customer.LastName}: {ex.Message}");
            }
        }

        return string.Join("\n", results);
    }

    private string CreateEmail(Customer customer)
    {
        var firstName = CleanString(customer.FirstName ?? "user");
        var lastName = CleanString(customer.LastName ?? "");
        var segment = CleanString(customer.Segment ?? "standard");
        var randomNum = _random.Next(1, 99999);

        if (!string.IsNullOrEmpty(lastName))
            return $"{firstName}{lastName}.{segment}{randomNum}@company.com".ToLower();

        return $"{firstName}.{segment}{randomNum}@company.com".ToLower();
    }

    private string CleanString(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "unknown";
        return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"[^a-zA-Z0-9]", "");
    }

    public async Task<object> GetCustomerUserList()
    {
        return await _context.Customers
            .Include(c => c.ApplicationUser)
            .Select(c => new
            {
                CustomerName = $"{c.FirstName} {c.LastName}",
                CustomerSegment = c.Segment,
                HasUser = c.ApplicationUser != null,
                Username = c.ApplicationUser != null ? c.ApplicationUser.UserName : "Chưa có",
                Email = c.ApplicationUser != null ? c.ApplicationUser.Email : "Chưa có"
            })
            .Take(50)
            .ToListAsync();
    }
}
