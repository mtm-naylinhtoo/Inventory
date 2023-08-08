using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Inventory.DAL;
using Inventory.Models;
using Inventory.ViewModels;
using PagedList;

namespace Inventory.Controllers
{
    public class SalePricesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: SalePrices
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var salePrices = db.SalePrices.Include(s => s.Category).Include(s => s.Product);

            // Filter out categories and products with IsDeleted flag set to false
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();

            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");

            if (!string.IsNullOrEmpty(search))
            {
                salePrices = salePrices.Where(s => s.Price.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all sale prices.
            }
            else
            {
                salePrices = salePrices.Where(s => !s.IsDeleted);
            }

            // Order by the Price column in ascending order
            salePrices = salePrices.OrderBy(s => s.Price);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedSalePrices = salePrices.ToPagedList(pageNumber, pageSize);

            return View(pagedSalePrices);
        }

        // GET: SalePrices/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalePrice salePrice = db.SalePrices.Find(id);
            if (salePrice == null || salePrice.IsDeleted)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(salePrice.CreatedUserId);
            string updatedUserName = GetUserNameById(salePrice.UpdatedUserId);
            string deletedUserName = GetUserNameById(salePrice.DeletedUserId);

            var viewModel = new SalePriceDetailViewModel
            {
                SalePrice = salePrice,
                CreatedUserName = createdUserName,
                UpdatedUserName = updatedUserName,
                DeletedUserName = deletedUserName
            };

            return View(viewModel);
        }

        // Helper method to get user name by user ID
        private string GetUserNameById(string userId)
        {
            if (userId == null)
            {
                return "Unknown User";
            }

            User user = db.Users.Find(userId);
            return user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";
        }

        // GET: SalePrices/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();

            ViewBag.CategoryId = new SelectList(categories, "ID", "Name");
            ViewBag.ProductId = new SelectList(products, "ID", "Name");
            return View();
        }

        // POST: SalePrices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,CategoryId,ProductId,Price,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] SalePrice salePrice)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                // Check if there is already a sale price for the given ProductId
                var existingSalePrice = db.SalePrices.FirstOrDefault(s => s.ProductId == salePrice.ProductId && !s.IsDeleted);
                if (existingSalePrice != null)
                {
                    ModelState.AddModelError("", "A sale price already exists for this product.");
                    // Fetch categories and products for the dropdowns
                    var categoriesList = db.Categories.Where(c => !c.IsDeleted).ToList();
                    var productsList = db.Products.Where(p => !p.IsDeleted).ToList();
                    ViewBag.CategoryId = new SelectList(categoriesList, "ID", "Name", salePrice.CategoryId);
                    ViewBag.ProductId = new SelectList(productsList, "ID", "Name", salePrice.ProductId);
                    return View(salePrice);
                }

                salePrice.ID = Guid.NewGuid().ToString();
                salePrice.IsActive = true;
                salePrice.CreatedDate = DateTime.Now;
                salePrice.CreatedUserId = currentUser.Id;
                db.SalePrices.Add(salePrice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Fetch categories and products for the dropdowns
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", salePrice.CategoryId);
            ViewBag.ProductId = new SelectList(products, "ID", "Name", salePrice.ProductId);
            return View(salePrice);
        }


        // GET: SalePrices/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalePrice salePrice = db.SalePrices.Find(id);
            if (salePrice == null || salePrice.IsDeleted)
            {
                return HttpNotFound();
            }

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();

            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", salePrice.CategoryId);
            ViewBag.ProductId = new SelectList(products, "ID", "Name", salePrice.ProductId);
            return View(salePrice);
        }

        // POST: SalePrices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,CategoryId,ProductId,Price,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] SalePrice salePrice)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];

                // Check if there is already a sale price for the given ProductId, excluding the current sale price being edited
                var existingSalePrice = db.SalePrices.FirstOrDefault(s => s.ProductId == salePrice.ProductId && s.ID != salePrice.ID && !s.IsDeleted);
                if (existingSalePrice != null)
                {
                    ModelState.AddModelError("", "A sale price already exists for this product.");
                    // Fetch categories and products for the dropdowns
                    var categoriesList = db.Categories.Where(c => !c.IsDeleted).ToList();
                    var productsList = db.Products.Where(p => !p.IsDeleted).ToList();
                    ViewBag.CategoryId = new SelectList(categoriesList, "ID", "Name", salePrice.CategoryId);
                    ViewBag.ProductId = new SelectList(productsList, "ID", "Name", salePrice.ProductId);
                    return View(salePrice);
                }

                salePrice.UpdatedDate = DateTime.Now;
                salePrice.UpdatedUserId = currentUser.Id;
                db.Entry(salePrice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Fetch categories and products for the dropdowns
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", salePrice.CategoryId);
            ViewBag.ProductId = new SelectList(products, "ID", "Name", salePrice.ProductId);
            return View(salePrice);
        }

        // GET: SalePrices/GetProductsByCategory
        [HttpGet]
        public ActionResult GetProductsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Convert the categoryId to a Guid data type (assuming it is a Guid based on your model)
            Guid parsedCategoryId;
            if (!Guid.TryParse(categoryId, out parsedCategoryId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch products from the database based on the selected categoryId
            var products = db.Products
                .Where(p => p.CategoryId == parsedCategoryId.ToString() && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToList();

            // Convert the products to a SelectList for rendering in the dropdown
            var productList = products.Select(p => new SelectListItem
            {
                Value = p.ID,
                Text = p.Name
            });

            return Json(productList, JsonRequestBehavior.AllowGet);
        }

        // GET: SalePrices/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalePrice salePrice = db.SalePrices.Find(id);
            if (salePrice == null || salePrice.IsDeleted)
            {
                return HttpNotFound();
            }

            return View(salePrice);
        }

        // POST: SalePrices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            SalePrice salePrice = db.SalePrices.Find(id);
            salePrice.DeletedUserId = currentUser.Id;
            salePrice.IsDeleted = true;
            salePrice.DeletedDate = DateTime.Now;
            db.Entry(salePrice).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
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
