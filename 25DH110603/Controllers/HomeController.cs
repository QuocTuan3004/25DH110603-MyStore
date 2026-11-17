using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using _25DH110603.Models;
using _25DH110603.Models.ViewModel;
using PagedList;

namespace _25DH110603.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var products = db.Products.Where(p => p.Quantity > 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                    p.ProductDescription.Contains(searchTerm) || p.Category.CategoryName.Contains(searchTerm));
            }

            var model = new HomeProductVM
            {
                SearchTerm = searchTerm,
                FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Sum(d => (int?)d.Quantity) ?? 0)
                    .ThenBy(p => p.ProductName).Take(10).ToList(),
                NewestProducts = products.OrderBy(p => p.OrderDetails.Sum(d => (int?)d.Quantity) ?? 0)
                    .ThenBy(p => p.ProductName).Take(20).ToPagedList(page ?? 1, 6)
            };
            return View(model);
        }

        public ActionResult ProductList(int? id, string searchTerm, int? page, string sortOrder)
        {
            var products = db.Products.Include(p => p.Category).Where(p => p.Quantity > 0).AsQueryable();
            if (id.HasValue) products = products.Where(p => p.CategoryID == id.Value);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                products = products.Where(p => p.ProductName.Contains(searchTerm) || p.ProductDescription.Contains(searchTerm));
            products = sortOrder == "price" ? products.OrderBy(p => p.ProductPrice) : products.OrderBy(p => p.ProductName);
            ViewBag.Category = id.HasValue ? db.Categories.Find(id.Value) : null;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;
            return View(products.ToPagedList(page ?? 1, 12));
        }

        public ActionResult ProductDetail(int id, int quantity = 1, int? page = 1)
        {
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null) return HttpNotFound();
            var model = new ProductDetailVM
            {
                Product = product,
                Quantity = quantity < 1 ? 1 : quantity,
                RelatedProducts = db.Products.Where(p => p.Quantity > 0 && p.CategoryID == product.CategoryID && p.ProductID != id).Take(8).ToList(),
                TopProducts = db.Products.Where(p => p.Quantity > 0 && p.CategoryID == product.CategoryID)
                    .OrderByDescending(p => p.OrderDetails.Sum(d => (int?)d.Quantity) ?? 0).Take(8).ToPagedList(page ?? 1, 3)
            };
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            return PartialView("_CategoriesPV", db.Categories.OrderBy(c => c.CategoryName).ToList());
        }

        [ChildActionOnly]
        public ActionResult PersonalMenu()
        {
            var cart = Session[CartService.SessionKey] as Cart;
            var vm = new PersonalMenuVM
            {
                IsLoggedIn = User.Identity.IsAuthenticated,
                Username = User.Identity.Name,
                CartCount = cart == null ? 0 : cart.Items.Sum(i => i.Quantity)
            };
            return PartialView("_PersonalMenuPV", vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
