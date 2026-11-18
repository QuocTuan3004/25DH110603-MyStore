using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH110603.Models;
namespace _25DH110603.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index(string customerName, string paymentStatus, DateTime? orderDate, int? page)
        {
            var query = db.Orders.Include(o => o.Customer).AsQueryable();
            if (!string.IsNullOrWhiteSpace(customerName)) query = query.Where(o => o.Customer.CustomerName.Contains(customerName));
            if (!string.IsNullOrWhiteSpace(paymentStatus)) query = query.Where(o => o.PaymentStatus == paymentStatus);
            if (orderDate.HasValue)
            {
                var start = orderDate.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(o => o.OrderDate >= start && o.OrderDate < end);
            }
            const int pageSize = 5;
            var currentPage = page.HasValue && page.Value > 0 ? page.Value : 1;
            var total = query.Count();
            ViewBag.CustomerName = customerName;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderDate = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.Page = currentPage;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            ViewBag.TotalOrders = total;
            return View(query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID)
                .Skip((currentPage - 1) * pageSize).Take(pageSize).ToList());
        }
        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(d => d.Product)).FirstOrDefault(o => o.OrderID == id.Value);
            if (order == null) return HttpNotFound();
            return View(order);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus)
        {
            var allowed = new[] { "Processing", "Shipping", "Delivered", "Cancelled" };
            if (!allowed.Contains(orderStatus)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();
            order.OrderStatus = orderStatus;
            db.SaveChanges();
            TempData["Message"] = "Đã cập nhật trạng thái đơn hàng.";
            return RedirectToAction("Details", new { id = id });
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
