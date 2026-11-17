using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using _25DH110603.Models;
using _25DH110603.Models.ViewModel;

namespace _25DH110603.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();

        public ActionResult Checkout()
        {
            var cart = new CartService(Session).GetCart();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) return RedirectToAction("ProfileInfo", "Account");
            return View(new CheckoutVM { ShippingAddress = customer.CustomerAddress, DeliveryMethod = "Tiêu chuẩn", PaymentMethod = "Thanh toán khi nhận hàng" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = new CartService(Session).GetCart();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");
            if (!ModelState.IsValid) return View(model);
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) return RedirectToAction("ProfileInfo", "Account");
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = System.DateTime.Today,
                TotalAmount = cart.Total,
                PaymentStatus = "Pending",
                ShippingAddress = model.ShippingAddress.Trim(),
                DeliveryMethod = model.DeliveryMethod,
                PaymentMethod = model.PaymentMethod
            };
            using (var transaction = db.Database.BeginTransaction())
            {
                foreach (var item in cart.Items)
                {
                    var product = db.Products.FirstOrDefault(p => p.ProductID == item.ProductID);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        transaction.Rollback();
                        TempData["CartError"] = "Một sản phẩm vừa hết hàng hoặc không đủ số lượng. Vui lòng kiểm tra lại giỏ hàng.";
                        return RedirectToAction("Index", "Cart");
                    }
                    product.Quantity -= item.Quantity;
                    order.OrderDetails.Add(new OrderDetail { ProductID = item.ProductID, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                }
                db.Orders.Add(order);
                db.SaveChanges();
                transaction.Commit();
            }
            new CartService(Session).Save(new Cart());
            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                .Include(o => o.Customer).FirstOrDefault(o => o.OrderID == id && o.Customer.Username == User.Identity.Name);
            return order == null ? (ActionResult)HttpNotFound() : View(order);
        }

        public ActionResult MyOrder(string searchTerm)
        {
            var orders = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                .Where(o => o.Customer.Username == User.Identity.Name);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                int orderId;
                if (int.TryParse(term, out orderId))
                    orders = orders.Where(o => o.OrderID == orderId || o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
                else
                    orders = orders.Where(o => o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
            }
            ViewBag.SearchTerm = searchTerm;
            return View(orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID).ToList());
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
