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
    public class ProductsController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Products
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var products = db.Products.Include(p => p.Category);

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Code.Contains(search) || p.Name.Contains(search) || p.Category.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all products.
            }
            else
            {
                products = products.Where(p => !p.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            products = products.OrderBy(p => p.SortOrder);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            return View(pagedProducts);
        }

        // GET: Products/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
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

            string createdUserName = GetUserNameById(product.CreatedUserId);
            string updatedUserName = GetUserNameById(product.UpdatedUserId);
            string deletedUserName = GetUserNameById(product.DeletedUserId);

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
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

        // GET: Products/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,CategoryId,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Product product)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                product.ID = Guid.NewGuid().ToString();
                product.IsActive = true;
                product.CreatedDate = DateTime.Now;
                product.CreatedUserId = currentUser.Id;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
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


            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,CategoryId,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Product product)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                product.UpdatedDate = DateTime.Now;
                product.UpdatedUserId = currentUser.Id;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            Product product = db.Products.Find(id);
            product.DeletedUserId = currentUser.Id;
            product.IsDeleted = true;
            product.DeletedDate = DateTime.Now;
            db.Entry(product).State = EntityState.Modified;
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
