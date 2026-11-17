using System.Collections.Generic;
using _25DH110603.Models;
namespace _25DH110603.Models.ViewModel
{
    public class InventoryVM
    {
        public IList<Product> InStock { get; set; }
        public IList<Product> NeedRestock { get; set; }
    }
}
