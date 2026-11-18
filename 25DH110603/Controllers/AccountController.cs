using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using _25DH110603.Models;
using _25DH110603.Models.ViewModel;

namespace _25DH110603.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();

        [AllowAnonymous]
        public ActionResult Register() { return View(new RegisterVM()); }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);
            var username = model.Username.Trim();
            if (db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng.");
                return View(model);
            }
            db.Users.Add(new User { Username = username, Password = model.Password, UserRole = "C" });
            db.Customers.Add(new Customer { Username = username, CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone, CustomerEmail = model.CustomerEmail, CustomerAddress = model.CustomerAddress });
            db.SaveChanges();
            FormsAuthentication.SetAuthCookie(username, false);
            TempData["Message"] = "Tạo tài khoản thành công. Chào mừng bạn đến với MyStore!";
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Login(bool registered = false) { ViewBag.Registered = registered; return View(new LoginVM()); }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);
            var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }
            FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            if (user.UserRole == "A") return RedirectToAction("Index", "Products", new { area = "Admin" });
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ProfileInfo()
        {
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) return HttpNotFound();
            return View(new ProfileVM { Username = customer.Username, CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone, CustomerEmail = customer.CustomerEmail, CustomerAddress = customer.CustomerAddress });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            if (!ModelState.IsValid) return View("ProfileInfo", model);
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) return HttpNotFound();
            customer.CustomerName = model.CustomerName; customer.CustomerPhone = model.CustomerPhone;
            customer.CustomerEmail = model.CustomerEmail; customer.CustomerAddress = model.CustomerAddress;
            db.SaveChanges();
            TempData["Message"] = "Thông tin tài khoản đã được cập nhật.";
            return RedirectToAction("ProfileInfo");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid) { TempData["PasswordError"] = "Vui lòng kiểm tra lại thông tin mật khẩu."; return RedirectToAction("ProfileInfo"); }
            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return HttpNotFound();
            if (user.Password != model.CurrentPassword) { TempData["PasswordError"] = "Mật khẩu hiện tại không chính xác."; return RedirectToAction("ProfileInfo"); }
            user.Password = model.NewPassword; db.SaveChanges();
            TempData["Message"] = "Mật khẩu đã được thay đổi.";
            return RedirectToAction("ProfileInfo");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Logout() { FormsAuthentication.SignOut(); return RedirectToAction("Index", "Home"); }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
