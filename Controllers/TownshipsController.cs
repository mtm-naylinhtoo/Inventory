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
    public class TownshipsController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Townships
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var townships = db.Townships.Include(t => t.City).Include(t => t.State);

            if (!string.IsNullOrEmpty(search))
            {
                townships = townships.Where(t => t.Code.Contains(search) || t.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all townships.
            }
            else
            {
                townships = townships.Where(t => !t.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            townships = townships.OrderBy(t => t.SortOrder);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedTownships = townships.ToPagedList(pageNumber, pageSize);

            return View(pagedTownships);
        }

        // GET: Townships/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Township township = db.Townships.Find(id);
            if (township == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(township.CreatedUserId);
            string updatedUserName = GetUserNameById(township.UpdatedUserId);
            string deletedUserName = GetUserNameById(township.DeletedUserId);

            var viewModel = new TownshipDetailViewModel
            {
                Township = township,
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

        // GET: Townships/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name");
            ViewBag.CityId = new SelectList(Enumerable.Empty<City>(), "ID", "Name");
            return View();
        }

        // POST: Townships/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,StateId,CityId,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Township township)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                township.ID = Guid.NewGuid().ToString();
                township.IsActive = true;
                township.CreatedDate = DateTime.Now;
                township.CreatedUserId = currentUser.Id;
                db.Townships.Add(township);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", township.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(x => !x.IsDeleted).Where(c => c.StateId == township.StateId), "ID", "Name");
            return View(township);
        }

        // GET: Townships/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Township township = db.Townships.Find(id);
            if (township == null)
            {
                return HttpNotFound();
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", township.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(x => !x.IsDeleted).Where(c => c.StateId == township.StateId), "ID", "Name", township.CityId);
            return View(township);
        }

        // POST: Townships/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,StateId,CityId,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Township township)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                township.UpdatedDate = DateTime.Now;
                township.UpdatedUserId = currentUser.Id;
                db.Entry(township).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", township.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(x => !x.IsDeleted).Where(c => c.StateId == township.StateId), "ID", "Name", township.CityId);
            return View(township);
        }
        // GET: Townships/GetCitiesByState
        [HttpGet]
        public ActionResult GetCitiesByState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Convert the stateId to a Guid data type (assuming it is a Guid based on your model)
            Guid parsedStateId;
            if (!Guid.TryParse(stateId, out parsedStateId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch cities from the database based on the selected stateId
            var cities = db.Cities.Where(x => !x.IsDeleted)
                .Where(c => c.StateId == parsedStateId.ToString() && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ToList();

            // Convert the cities to a SelectList for rendering in the dropdown
            var cityList = cities.Select(c => new SelectListItem
            {
                Value = c.ID, // Assuming your City model has the ID property as a string
                Text = c.Name // Assuming your City model has the Name property
            });

            return Json(cityList, JsonRequestBehavior.AllowGet);
        }

        // GET: Townships/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Township township = db.Townships.Find(id);
            if (township == null)
            {
                return HttpNotFound();
            }

            return View(township);
        }

        // POST: Townships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            Township township = db.Townships.Find(id);
            township.DeletedUserId = currentUser.Id;
            township.IsDeleted = true;
            township.DeletedDate = DateTime.Now;
            db.Entry(township).State = EntityState.Modified;
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
