using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110603.Models;
using _25DH110603.Models.ViewModel;
using PagedList;

namespace _25DH110603.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice,
                          string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                                            || p.ProductDescription.Contains(searchTerm)
                                            || p.Category.CategoryName.Contains(searchTerm));
            }
            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            model.SortOrder = sortOrder;
            switch (sortOrder)
            {
                case "name_asc": products = products.OrderBy(p => p.ProductName); break;
                case "name_desc": products = products.OrderByDescending(p => p.ProductName); break;
                case "price_asc": products = products.OrderBy(p => p.ProductPrice); break;
                case "price_desc": products = products.OrderByDescending(p => p.ProductPrice); break;
                default: products = products.OrderBy(p => p.ProductID); break;
            }

            model.PageNumber = page ?? 1;
            model.PageSize = 5;
            model.Products = products.ToPagedList(model.PageNumber, model.PageSize);

            return View(model);
        }

        public ActionResult Inventory()
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.OrderDetails).OrderBy(p => p.ProductName).ToList();
            return View(new InventoryVM
            {
                InStock = products.Where(p => p.Quantity > 0).ToList(),
                NeedRestock = products.Where(p => p.Quantity == 0).ToList()
            });
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(new Product { Quantity = 0 });
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductPrice,ProductImage,ProductDescription,Quantity")] Product product, HttpPostedFileBase ImageFile, string ImageUrl)
        {
            string uploadedPath = null;
            string resolvedImageUrl = null;
            bool hasUploadedImage = ImageFile != null && ImageFile.ContentLength > 0;
            if (ModelState.IsValid && !TrySaveProductImage(ImageFile, out uploadedPath))
                ModelState.AddModelError("ImageFile", "Chỉ nhận ảnh JPG, PNG, GIF hoặc WEBP tối đa 5 MB.");
            if (ModelState.IsValid && !hasUploadedImage && !TryResolveProductImageUrl(ImageUrl, out resolvedImageUrl))
                ModelState.AddModelError("ImageUrl", "Vui lòng nhập URL ảnh hợp lệ bắt đầu bằng http:// hoặc https://.");
            if (ModelState.IsValid)
            {
                product.ProductImage = hasUploadedImage ? uploadedPath : resolvedImageUrl;
                db.Products.Add(product);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException exception)
                {
                    foreach (var entityError in exception.EntityValidationErrors)
                    {
                        foreach (var validationError in entityError.ValidationErrors)
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductPrice,ProductImage,ProductDescription,Quantity")] Product product, HttpPostedFileBase ImageFile, string ImageUrl)
        {
            string uploadedPath = null;
            string resolvedImageUrl = null;
            bool hasUploadedImage = ImageFile != null && ImageFile.ContentLength > 0;
            if (ModelState.IsValid && !TrySaveProductImage(ImageFile, out uploadedPath))
                ModelState.AddModelError("ImageFile", "Chỉ nhận ảnh JPG, PNG, GIF hoặc WEBP tối đa 5 MB.");
            if (ModelState.IsValid && !hasUploadedImage && !String.IsNullOrWhiteSpace(ImageUrl) && !TryResolveProductImageUrl(ImageUrl, out resolvedImageUrl))
                ModelState.AddModelError("ImageUrl", "Vui lòng nhập URL ảnh hợp lệ bắt đầu bằng http:// hoặc https://.");
            if (ModelState.IsValid)
            {
                var existing = db.Products.Find(product.ProductID);
                if (existing == null) return HttpNotFound();
                existing.CategoryID = product.CategoryID;
                existing.ProductName = product.ProductName;
                existing.ProductDescription = product.ProductDescription;
                existing.ProductPrice = product.ProductPrice;
                existing.Quantity = product.Quantity;
                if (hasUploadedImage) existing.ProductImage = uploadedPath;
                else if (!String.IsNullOrWhiteSpace(resolvedImageUrl)) existing.ProductImage = resolvedImageUrl;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Include(p => p.OrderDetails).FirstOrDefault(p => p.ProductID == id);
            if (product == null) return HttpNotFound();
            if (product.OrderDetails.Any())
            {
                ModelState.AddModelError("", "Không thể xóa sản phẩm đã xuất hiện trong đơn hàng. Hãy giữ sản phẩm để bảo toàn lịch sử giao dịch.");
                return View("Delete", product);
            }
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool TryResolveProductImageUrl(string imageUrl, out string resolvedImageUrl)
        {
            resolvedImageUrl = null;
            if (String.IsNullOrWhiteSpace(imageUrl)) return true;

            imageUrl = imageUrl.Trim();
            Uri uri;
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return false;

            resolvedImageUrl = imageUrl;
            return true;
        }

        private bool TrySaveProductImage(HttpPostedFileBase file, out string relativePath)
        {
            relativePath = null;
            if (file == null || file.ContentLength == 0) return true;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension) || file.ContentLength > 5 * 1024 * 1024) return false;
            var folder = Server.MapPath("~/Content/Images/Products");
            Directory.CreateDirectory(folder);
            var fileName = Guid.NewGuid().ToString("N") + extension;
            file.SaveAs(Path.Combine(folder, fileName));
            relativePath = "~/Content/Images/Products/" + fileName;
            return true;
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
