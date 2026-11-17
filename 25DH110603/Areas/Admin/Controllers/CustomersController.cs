using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH110603.Models;
namespace _25DH110603.Areas.Admin.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index() => View(db.Customers.Include(c => c.User).OrderBy(c => c.CustomerName).ToList());
        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var customer = db.Customers.Include(c => c.Orders).FirstOrDefault(c => c.CustomerID == id.Value);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
