using System.ComponentModel.DataAnnotations;
namespace _25DH110603.Models.ViewModel
{
    public class AdminUserVM
    {
        public string OriginalUsername { get; set; }
        [Required, StringLength(255), Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        [Required, StringLength(50, MinimumLength = 6), DataType(DataType.Password), Display(Name = "Mật khẩu")]
        public string Password { get; set; }
    }
}
