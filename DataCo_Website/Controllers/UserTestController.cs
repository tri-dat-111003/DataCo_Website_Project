using Microsoft.AspNetCore.Mvc;

public class UserTestController : Controller
{
    private readonly UserGenerationService _userService;

    public UserTestController(UserGenerationService userService)
    {
        _userService = userService;
    }

    // Trang để xem và tạo user
    public IActionResult Index()
    {
        return View();
    }

    // API để tạo user cho tất cả customer
    [HttpPost]
    public async Task<IActionResult> CreateUsers()
    {
        try
        {
            var result = await _userService.CreateUsersForAllCustomers();
            return Json(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // API để xem danh sách customer và user
    [HttpGet]
    public async Task<IActionResult> GetCustomerUsers()
    {
        try
        {
            var list = await _userService.GetCustomerUserList();
            return Json(new { success = true, data = list });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}