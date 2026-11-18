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
                OrderStatus = "Processing",
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

        public ActionResult Details(int id)
        {
            var order = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                .Include(o => o.Customer).FirstOrDefault(o => o.OrderID == id && o.Customer.Username == User.Identity.Name);
            return order == null ? (ActionResult)HttpNotFound() : View(order);
        }

        public ActionResult MyOrder(string searchTerm, string status, string expandedStatus)
        {
            var userOrders = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                .Where(o => o.Customer.Username == User.Identity.Name);
            var hasAnyOrders = userOrders.Any();
            var filtered = userOrders;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                int orderId;
                if (int.TryParse(term, out orderId))
                    filtered = filtered.Where(o => o.OrderID == orderId || o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
                else
                    filtered = filtered.Where(o => o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
            }

            var statuses = new[] { "all", "waiting", "processing", "shipping", "delivered", "cancelled" };
            if (string.IsNullOrWhiteSpace(status) || !statuses.Contains(status.ToLowerInvariant())) status = "all";
            if (status == "waiting") filtered = filtered.Where(o => (o.PaymentStatus == "Pending" || o.PaymentStatus == "Chưa thanh toán") && o.OrderStatus != "Cancelled");
            else if (status != "all") filtered = filtered.Where(o => o.OrderStatus == status);

            var groups = new System.Collections.Generic.List<MyOrderGroupVM>();
            var groupDefinitions = new[] {
                new { Key = "waiting", Label = "Chờ thanh toán" },
                new { Key = "processing", Label = "Đang xử lý" },
                new { Key = "shipping", Label = "Đang giao" },
                new { Key = "delivered", Label = "Đã giao" },
                new { Key = "cancelled", Label = "Đã hủy" }
            };
            foreach (var definition in groupDefinitions)
            {
                var groupQuery = status == "waiting"
                    ? filtered
                    : status == "all" && definition.Key == "waiting"
                        ? filtered.Where(o => (o.PaymentStatus == "Pending" || o.PaymentStatus == "Chưa thanh toán") && o.OrderStatus != "Cancelled")
                    : status == "all" ? filtered.Where(o => o.OrderStatus == definition.Key)
                    : status == definition.Key ? filtered : filtered.Where(o => false);
                var count = groupQuery.Count();
                if (count == 0) continue;
                var expanded = string.Equals(expandedStatus, definition.Key, System.StringComparison.OrdinalIgnoreCase);
                groups.Add(new MyOrderGroupVM {
                    StatusKey = definition.Key, StatusLabel = definition.Label, TotalCount = count, IsExpanded = expanded,
                    Orders = groupQuery.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID)
                        .Take(expanded ? 50 : 2).ToList()
                });
                if (status != "all") break;
            }
            return View(new MyOrderVM { SearchTerm = searchTerm, StatusFilter = status,
                ExpandedStatus = expandedStatus, HasAnyOrders = hasAnyOrders, Groups = groups });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Reorder(int id)
        {
            var order = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id && o.Customer.Username == User.Identity.Name);
            if (order == null) return HttpNotFound();
            var cartService = new CartService(Session);
            var cart = cartService.GetCart();
            var orderedProductIds = order.OrderDetails.Select(d => d.ProductID).Distinct().ToList();
            foreach (var productId in orderedProductIds)
            {
                var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == productId);
                var existing = cart.Items.FirstOrDefault(i => i.ProductID == productId);
                if (product == null || product.Quantity < (existing == null ? 1 : existing.Quantity + 1))
                {
                    TempData["CartError"] = "Một sản phẩm trong đơn đã hết hàng hoặc không đủ tồn kho. Giỏ hàng chưa được cập nhật.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            foreach (var productId in orderedProductIds)
            {
                var product = db.Products.Include(p => p.Category).First(p => p.ProductID == productId);
                cartService.Add(product, 1);
            }
            TempData["Message"] = "Đã thêm mỗi sản phẩm trong đơn vào giỏ hàng.";
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var order = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id && o.Customer.Username == User.Identity.Name);
            if (order == null) return HttpNotFound();
            if (order.OrderStatus != "Processing" ||
                (order.PaymentStatus != "Pending" && order.PaymentStatus != "Chưa thanh toán"))
            {
                TempData["OrderMessage"] = "Chỉ có thể hủy đơn đang chờ thanh toán.";
                return RedirectToAction("MyOrder", new { status = "all" });
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = db.Products.FirstOrDefault(p => p.ProductID == detail.ProductID);
                    if (product != null) product.Quantity += detail.Quantity;
                }
                order.OrderStatus = "Cancelled";
                db.SaveChanges();
                transaction.Commit();
            }
            TempData["OrderMessage"] = "Đơn hàng đã được hủy và số lượng sản phẩm đã được trả lại kho.";
            return RedirectToAction("MyOrder", new { status = "all" });
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
