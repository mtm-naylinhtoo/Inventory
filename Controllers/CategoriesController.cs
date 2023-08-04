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
    public class CategoriesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Categories
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var categories = db.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                categories = categories.Where(c => c.Code.Contains(search) || c.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all categories.
            }
            else
            {
                categories = categories.Where(c => !c.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            categories = categories.OrderBy(c => c.SortOrder);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedCategories = categories.ToPagedList(pageNumber, pageSize);

            return View(pagedCategories);
        }

        // GET: Categories/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(category.CreatedUserId);
            string updatedUserName = GetUserNameById(category.UpdatedUserId);
            string deletedUserName = GetUserNameById(category.DeletedUserId);

            var viewModel = new CategoryDetailViewModel
            {
                Category = category,
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

        // GET: Categories/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Category category)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                category.ID = Guid.NewGuid().ToString();
                category.IsActive = true;
                category.CreatedDate = DateTime.Now;
                category.CreatedUserId = currentUser.Id;
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Category category)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                category.UpdatedDate = DateTime.Now;
                category.UpdatedUserId = currentUser.Id;
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            Category category = db.Categories.Find(id);
            category.DeletedUserId = currentUser.Id;
            category.IsDeleted = true;
            category.DeletedDate = DateTime.Now;
            db.Entry(category).State = EntityState.Modified;
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
