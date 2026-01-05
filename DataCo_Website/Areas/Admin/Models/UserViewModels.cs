using System.ComponentModel.DataAnnotations;

namespace DataCo_Website.Areas.Admin.Models
{
    // ViewModel hiển thị danh sách - SIÊU NHẸ
    public class UserListViewModel
    {
        public string UserId { get; set; }
        public int? CustomerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool IsActive { get; set; }
    }

    // ViewModel đổi mật khẩu
    public class ChangePasswordViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }

    // ViewModel Edit User
    public class EditUserViewModel
    {
        public string UserId { get; set; }
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string PhoneNumber { get; set; } // Không bắt buộc

        [Display(Name = "EmailConfirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "PhoneNumberConfirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "TwoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "LockoutEnabled")]
        public bool LockoutEnabled { get; set; }

        // Bỏ IsActive - dùng Delete/Restore thay thế
    }
}