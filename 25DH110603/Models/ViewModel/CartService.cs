using System.Linq;
using System.Web;
using _25DH110603.Models;

namespace _25DH110603.Models.ViewModel
{
    public class CartService
    {
        public const string SessionKey = "Cart";
        private readonly HttpSessionStateBase session;
        public CartService(HttpSessionStateBase session) { this.session = session; }
        public Cart GetCart() { return session[SessionKey] as Cart ?? new Cart(); }
        public void Save(Cart cart) { session[SessionKey] = cart; }
        public void Add(Product product, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductID == product.ProductID);
            if (item == null)
                cart.Items.Add(new CartItem { ProductID = product.ProductID, ProductName = product.ProductName,
                    ProductImage = product.ProductImage, Category = product.Category == null ? "Khác" : product.Category.CategoryName,
                    UnitPrice = product.ProductPrice, Quantity = quantity, StockQuantity = product.Quantity });
            else { item.Quantity += quantity; item.StockQuantity = product.Quantity; }
            Save(cart);
        }
    }
}
