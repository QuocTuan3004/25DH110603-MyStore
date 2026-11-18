using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH110603.Models;
using _25DH110603.Models.ViewModel;
namespace _25DH110603.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class UsersController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index() => View(db.Users.Where(u => u.UserRole == "A").OrderBy(u => u.Username).ToList());
        public ActionResult Create() => View(new AdminUserVM());
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(AdminUserVM model)
        {
            if (!ModelState.IsValid) return View(model);
            model.Username = model.Username.Trim();
            if (db.Users.Any(u => u.Username == model.Username)) { ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại."); return View(model); }
            db.Users.Add(new User { Username = model.Username, Password = model.Password, UserRole = "A" });
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = db.Users.FirstOrDefault(u => u.Username == id && u.UserRole == "A");
            if (user == null) return HttpNotFound();
            return View(new AdminUserVM { OriginalUsername = user.Username, Username = user.Username });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(AdminUserVM model)
        {
            if (string.IsNullOrWhiteSpace(model.Password)) ModelState.Remove("Password");
            if (!ModelState.IsValid) return View(model);
            var user = db.Users.FirstOrDefault(u => u.Username == model.OriginalUsername && u.UserRole == "A");
            if (user == null) return HttpNotFound();
            var username = (model.Username ?? "").Trim();
            if (db.Users.Any(u => u.Username == username && u.Username != model.OriginalUsername)) { ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại."); return View(model); }
            user.Username = username;
            if (!string.IsNullOrWhiteSpace(model.Password)) user.Password = model.Password;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = db.Users.FirstOrDefault(u => u.Username == id && u.UserRole == "A");
            return user == null ? (ActionResult)HttpNotFound() : View(user);
        }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == id && u.UserRole == "A");
            if (user == null) return HttpNotFound();
            if (user.Username == User.Identity.Name || db.Users.Count(u => u.UserRole == "A") <= 1)
            {
                ModelState.AddModelError("", "Không thể xóa tài khoản đang đăng nhập hoặc tài khoản quản trị cuối cùng.");
                return View("Delete", user);
            }
            db.Users.Remove(user); db.SaveChanges();
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
