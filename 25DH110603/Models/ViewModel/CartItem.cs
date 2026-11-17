namespace _25DH110603.Models.ViewModel
{
    [System.Serializable]
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
    }
}
