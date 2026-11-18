using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using _25DH110603.Models;

namespace _25DH110603.Models.ViewModel
{
    public class RegisterVM
    {
        [Required, StringLength(255), Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        [Required, StringLength(50, MinimumLength = 6), DataType(DataType.Password), Display(Name = "Mật khẩu")]
        public string Password { get; set; }
        [Required, Compare("Password"), DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
        [Required, Display(Name = "Họ tên")]
        public string CustomerName { get; set; }
        [Required, Phone, StringLength(15), Display(Name = "Điện thoại")]
        public string CustomerPhone { get; set; }
        [EmailAddress, Display(Name = "Email")]
        public string CustomerEmail { get; set; }
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }
    }

    public class LoginVM
    {
        [Required, Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "Mật khẩu")]
        public string Password { get; set; }
        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }

    public class ProfileVM
    {
        public string Username { get; set; }
        [Required, Display(Name = "Họ tên")]
        public string CustomerName { get; set; }
        [Required, Phone, StringLength(15), Display(Name = "Điện thoại")]
        public string CustomerPhone { get; set; }
        [EmailAddress, Display(Name = "Email")]
        public string CustomerEmail { get; set; }
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required, DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }
        [Required, StringLength(50, MinimumLength = 6), DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }
        [Required, Compare("NewPassword"), DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; }
    }

    public class MyOrderVM
    {
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public string ExpandedStatus { get; set; }
        public bool HasAnyOrders { get; set; }
        public List<MyOrderGroupVM> Groups { get; set; }
    }

    public class MyOrderGroupVM
    {
        public string StatusKey { get; set; }
        public string StatusLabel { get; set; }
        public int TotalCount { get; set; }
        public bool IsExpanded { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class CheckoutVM
    {
        [Required, Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }
        [Required, Display(Name = "Hình thức giao hàng")]
        public string DeliveryMethod { get; set; }
        [Required, Display(Name = "Hình thức thanh toán")]
        public string PaymentMethod { get; set; }
    }
}
