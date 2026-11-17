using System.Collections.Generic;
using System.Linq;

namespace _25DH110603.Models.ViewModel
{
    [System.Serializable]
    public class Cart
    {
        public Cart() { Items = new List<CartItem>(); }
        public List<CartItem> Items { get; set; }
        public IEnumerable<IGrouping<string, CartItem>> GroupedItems { get { return Items.GroupBy(i => i.Category ?? "Khác"); } }
        public decimal Total { get { return Items.Sum(i => i.LineTotal); } }
        public int Count { get { return Items.Sum(i => i.Quantity); } }
    }
}
