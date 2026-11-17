using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BE_0402_v2.Models;
using BE_0402_v2.Models.ViewModels;
using _25DH110603.Models;


namespace _25DH110603.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private MyStore2Entities db = new MyStore2Entities();
        // GET: Admin/Product
        public ActionResult Index(string SearchTerm)
        {
            // var products = db.Products.Include(p => p.Category);
            // return View(Product.Tolist());

            // Khai báo biến 
            var model = new ProductSearchVM();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {// tìm kiếm sản phẩm theo tên, mô tả danh mục sản phẩm 
                products = products.Where(p =>
                    pro

            }
        }
    }
}