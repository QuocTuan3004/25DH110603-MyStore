using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110603.Models;

namespace _25DH110603.Areas.Admin.Controllers
{
    public sealed class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false;
            var username = httpContext.User.Identity.Name;
            using (var db = new MyStoreEntities())
                return db.Users.Any(user => user.Username == username && user.UserRole == "A");
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Tài khoản không có quyền truy cập khu vực quản trị.");
            else
                base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
