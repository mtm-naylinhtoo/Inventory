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
    public class CitiesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Cities
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var cities = db.Cities.Include(c => c.State);

            if (!string.IsNullOrEmpty(search))
            {
                cities = cities.Where(c => c.Code.Contains(search) || c.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all cities.
            }
            else
            {
                cities = cities.Where(c => !c.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            cities = cities.OrderBy(c => c.SortOrder);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedCities = cities.ToPagedList(pageNumber, pageSize);

            return View(pagedCities);
        }

        // GET: Cities/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            City city = db.Cities.Find(id);
            if (city == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(city.CreatedUserId);
            string updatedUserName = GetUserNameById(city.UpdatedUserId);
            string deletedUserName = GetUserNameById(city.DeletedUserId);

            var viewModel = new CityDetailViewModel
            {
                City = city,
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

        // GET: Cities/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name");
            return View();
        }

        // POST: Cities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,StateId,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] City city)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                city.ID = Guid.NewGuid().ToString();
                city.IsActive = true;
                city.CreatedDate = DateTime.Now;
                city.CreatedUserId = currentUser.Id;
                db.Cities.Add(city);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", city.StateId);
            return View(city);
        }

        // GET: Cities/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            City city = db.Cities.Find(id);
            if (city == null)
            {
                return HttpNotFound();
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", city.StateId);
            return View(city);
        }

        // POST: Cities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,StateId,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] City city)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                city.UpdatedDate = DateTime.Now;
                city.UpdatedUserId = currentUser.Id;
                db.Entry(city).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", city.StateId);
            return View(city);
        }

        // GET: Cities/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            City city = db.Cities.Find(id);
            if (city == null)
            {
                return HttpNotFound();
            }

            return View(city);
        }

        // POST: Cities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            City city = db.Cities.Find(id);
            city.DeletedUserId = currentUser.Id;
            city.IsDeleted = true;
            city.DeletedDate = DateTime.Now;
            db.Entry(city).State = EntityState.Modified;
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
