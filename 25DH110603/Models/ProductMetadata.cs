using System.ComponentModel.DataAnnotations;

namespace _25DH110603.Models
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        public string ProductDescription { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal ProductPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục sản phẩm.")]
        public int CategoryID { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải từ 0 trở lên.")]
        public int Quantity { get; set; }
    }
}