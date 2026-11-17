using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110603.Models;

namespace _25DH110603.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Categories
        // GET: Lâyd duữ liệu từ bảng Category trong DB để hiển thị lên 
        public ActionResult Index()
        {
            return View(db.Categories.Include(c => c.Products).OrderBy(c => c.CategoryName).ToList());
        }

        // GET: Admin/Categories/Details/5
        // Details: Lấy chi tiết một bản ghi có CategoryID = id 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);// mã lỗi 400: thiếu giá trị chuyền vào 
            }
            Category category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryID == id.Value);
            if (category == null)// không tìm thấy bản ghi nào 
            {
                return HttpNotFound();// mã lỗi 404 
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        // Load form Create 
        //[HttpGet] là phương thức mặc định, nên không cần khai báo 
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // POST lưu dữ liệu nhập từ form Craete vào DB 
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        // GET: Lấy dữ liệu của một danh mục đã có sao cho CategoryID = id 
        public ActionResult Edit(int? id)
        {
            return Details(id);
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //Category category = db.Categories.Find(id);
            //if (category == null)
            //{
            //    return HttpNotFound();
            //}
            //return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.Categories.Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryID == id.Value);
            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductCount = category.Products.Count;
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories.Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductCount = category.Products.Count;
            if (category.Products.Any())
            {
                ModelState.AddModelError("", "Không thể xóa danh mục khi vẫn còn sản phẩm thuộc danh mục này. Hãy xóa hoặc chuyển các sản phẩm trước.");
                return View("Delete", category);
            }

            try
            {
                db.Categories.Remove(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                ModelState.AddModelError("", "Không thể xóa danh mục vì dữ liệu khác vẫn đang sử dụng danh mục này.");
                return View("Delete", category);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
