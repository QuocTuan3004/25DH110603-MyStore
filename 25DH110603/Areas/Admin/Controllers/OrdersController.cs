using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH110603.Models;
namespace _25DH110603.Areas.Admin.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID).ToList();
            return View(orders);
        }
        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(d => d.Product)).FirstOrDefault(o => o.OrderID == id.Value);
            if (order == null) return HttpNotFound();
            return View(order);
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
