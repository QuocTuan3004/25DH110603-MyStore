using System.Linq;
using System.Web.Mvc;
using _25DH110603.Models;
using _25DH110603.Models.ViewModel;

namespace _25DH110603.Controllers
{
    public class CartController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();
        private CartService Service { get { return new CartService(Session); } }
        public ActionResult Index() { return View(Service.GetCart()); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddItem(int productId, int quantity = 1)
        {
            var product = db.Products.Find(productId);
            if (product == null) return HttpNotFound();
            var requested = quantity < 1 ? 1 : quantity;
            var existing = Service.GetCart().Items.FirstOrDefault(i => i.ProductID == productId);
            if (product.Quantity <= 0 || requested + (existing == null ? 0 : existing.Quantity) > product.Quantity)
            {
                TempData["CartError"] = "Số lượng yêu cầu vượt quá lượng hàng còn lại.";
                return RedirectToAction("ProductDetail", "Home", new { id = productId });
            }
            Service.Add(product, requested);
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = Service.GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                var product = db.Products.Find(productId);
                if (quantity <= 0) cart.Items.Remove(item);
                else if (product == null || quantity > product.Quantity) TempData["CartError"] = "Số lượng vượt quá hàng còn lại.";
                else { item.Quantity = quantity; item.StockQuantity = product.Quantity; }
                Service.Save(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RemoveItem(int productId)
        {
            var cart = Service.GetCart();
            cart.Items.RemoveAll(i => i.ProductID == productId);
            Service.Save(cart);
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ClearCart() { Service.Save(new Cart()); return RedirectToAction("Index"); }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
