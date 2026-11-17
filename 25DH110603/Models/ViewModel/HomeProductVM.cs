using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace _25DH110603.Models.ViewModel
{
    public class HomeProductVM
    {
        //tieeu chi ddeer search
        public string SearchTerm { get; set; }

        //tieeu chis ddeer paging
        public int PageNumber { get; set; }     // trang hiện tại
        public int PageSize { get; set; }       // số sản phẩm mỗi trang

        // Danh sách sản phẩm nooir baatj
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới nhất ddax phaan trang 
        public PagedList.IPagedList<Product> NewestProducts { get; set; }
    }
}