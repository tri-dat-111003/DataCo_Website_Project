using DataCo_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ✅ TỐI ƯU 1: DbContext với Connection Pooling & Query Tracking
builder.Services.AddDbContext<DataCoTestContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(300); // Giảm từ 300 xuống 60 giây
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3); // Auto retry khi lỗi
        sqlOptions.MaxBatchSize(100); // Tối ưu batch operations
    });

    // Tắt tracking mặc định (vì đã dùng AsNoTracking ở controller)
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Cấu hình Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<DataCoTestContext>();

// Cấu hình Cookie cho đăng nhập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// ✅ TỐI ƯU 2: Response Compression (Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/html",
        "text/css",
        "application/javascript",
        "text/javascript",
        "application/json",
        "text/json"
    });
});

// Cấu hình compression level
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest; // Optimal cho production
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// ✅ TỐI ƯU 3: Response Caching
builder.Services.AddResponseCaching();

// ✅ TỐI ƯU 4: Memory Cache cho static data
builder.Services.AddMemoryCache();

// ✅ TỐI ƯU 5: Output Caching (nếu dùng .NET 7+)
builder.Services.AddOutputCache();

// AddControllersWithViews
builder.Services.AddControllersWithViews();

// Thêm service
builder.Services.AddScoped<UserGenerationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ TỐI ƯU 6: Response Compression - ĐẶT TRƯỚC STATIC FILES
app.UseResponseCompression();

app.UseHttpsRedirection();

// ✅ TỐI ƯU 7: Static Files với Cache Headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files trong 7 ngày
        const int durationInSeconds = 60 * 60 * 24 * 7; // 7 days
        ctx.Context.Response.Headers.Append(
            "Cache-Control",
            $"public,max-age={durationInSeconds}"
        );
    }
});

app.UseRouting();

// ✅ TỐI ƯU 8: Response Caching Middleware
app.UseResponseCaching();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ TỐI ƯU 9: Middleware chặn role - Cải thiện performance
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";

    // Skip kiểm tra cho static files
    if (path.StartsWith("/css") || path.StartsWith("/js") ||
        path.StartsWith("/lib") || path.StartsWith("/images"))
    {
        await next();
        return;
    }

    // Danh sách các đường dẫn dành cho Customer
    var customerPaths = new[]
    {
        "/products",
        "/categories",
        "/cart",
        "/shop"
    };

    // Nếu ADMIN cố vào trang customer
    if (context.User.IsInRole("Admin") &&
        customerPaths.Any(p => path.StartsWith(p)))
    {
        context.Response.Redirect("/Admin");
        return;
    }

    // Nếu USER cố vào trang admin
    if (context.User.IsInRole("User") && path.StartsWith("/admin"))
    {
        context.Response.Redirect("/Products");
        return;
    }

    await next();
});

// ✅ TỐI ƯU 10: Output Caching (nếu dùng .NET 7+)
app.UseOutputCache();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "category_by_slug",
    pattern: "department/{departmentSlug}/category/{categorySlug}",
    defaults: new { controller = "Products", action = "IndexBySlug" });

app.MapControllerRoute(
    name: "product_details_slug",
    pattern: "department/{departmentSlug}/category/{categorySlug}/product/{productSlug}",
    defaults: new { controller = "Products", action = "DetailsBySlug" });

app.MapControllerRoute(
    name: "product_add_to_cart_slug",
    pattern: "department/{departmentSlug}/category/{categorySlug}/product/{productSlug}/add_to_cart",
    defaults: new { controller = "Cart", action = "AddToCartBySlug" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();