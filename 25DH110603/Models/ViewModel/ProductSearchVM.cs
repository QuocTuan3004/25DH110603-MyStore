using _25DH110603.Models;
using System.Collections.Generic;

namespace _25DH110603.Models.ViewModel
{
    public class ProductSearchVM
    {
        // Search theo tên, mô tả, sản phẩm 
        public string SearchTerm { get; set; }
        // Search theo giá
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        // Search theo thứ tự sắp xếp
        public string SortOrder { get; set; }
        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }     // trang hiện tại
        public int PageSize { get; set; }       // số sản phẩm mỗi trang

        // Danh sách sản phẩm   
        public PagedList.IPagedList<Product> Products { get; set; }
    }
}