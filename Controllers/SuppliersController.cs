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
    public class SuppliersController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Suppliers
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var suppliers = db.Suppliers.Include(s => s.City).Include(s => s.State).Include(s => s.Township);

            if (!string.IsNullOrEmpty(search))
            {
                suppliers = suppliers.Where(s => s.Code.Contains(search) || s.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all suppliers.
            }
            else
            {
                suppliers = suppliers.Where(s => !s.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            suppliers = suppliers.OrderBy(s => s.Name);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedSuppliers = suppliers.ToPagedList(pageNumber, pageSize);

            return View(pagedSuppliers);
        }

        // GET: Suppliers/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(supplier.CreatedUserId);
            string updatedUserName = GetUserNameById(supplier.UpdatedUserId);
            string deletedUserName = GetUserNameById(supplier.DeletedUserId);

            var viewModel = new SupplierDetailViewModel
            {
                Supplier = supplier,
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

        // GET: Suppliers/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.UserId = currentUser.Id;
            ViewBag.CreatedDate = DateTime.Now;
            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name");
            ViewBag.CityId = new SelectList(Enumerable.Empty<City>(), "ID", "Name");
            ViewBag.TownshipId = new SelectList(Enumerable.Empty<Township>(), "ID", "Name");
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,StateId,CityId,TownshipId,Code,Name,Mobile,Address,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Supplier supplier)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                supplier.Id = Guid.NewGuid().ToString();
                supplier.IsActive = true;
                supplier.CreatedDate = DateTime.Now;
                supplier.CreatedUserId = currentUser.Id;
                db.Suppliers.Add(supplier);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", supplier.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(x => !x.IsDeleted).Where(c => c.StateId == supplier.StateId), "ID", "Name", supplier.CityId);
            ViewBag.TownshipId = new SelectList(db.Townships.Where(x => !x.IsDeleted).Where(t => t.CityId == supplier.CityId), "ID", "Name", supplier.TownshipId);
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", supplier.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(x => !x.IsDeleted).Where(c => c.StateId == supplier.StateId), "ID", "Name", supplier.CityId);
            ViewBag.TownshipId = new SelectList(db.Townships.Where(x => !x.IsDeleted).Where(t => t.CityId == supplier.CityId), "ID", "Name", supplier.TownshipId);
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,StateId,CityId,TownshipId,Code,Name,Mobile,Address,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                supplier.UpdatedDate = DateTime.Now;
                supplier.UpdatedUserId = currentUser.Id;
                db.Entry(supplier).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States.Where(x => !x.IsDeleted), "ID", "Name", supplier.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(x => !x.IsDeleted).Where(c => c.StateId == supplier.StateId), "ID", "Name", supplier.CityId);
            ViewBag.TownshipId = new SelectList(db.Townships.Where(x => !x.IsDeleted).Where(t => t.CityId == supplier.CityId), "ID", "Name", supplier.TownshipId);
            return View(supplier);
        }

        // GET: Suppliers/GetCitiesByState
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

        // GET: Suppliers/GetTownshipsByCity
        [HttpGet]
        public ActionResult GetTownshipsByCity(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Convert the cityId to a Guid data type (assuming it is a Guid based on your model)
            Guid parsedCityId;
            if (!Guid.TryParse(cityId, out parsedCityId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch townships from the database based on the selected cityId
            var townships = db.Townships.Where(x => !x.IsDeleted)
                .Where(t => t.CityId == parsedCityId.ToString() && !t.IsDeleted)
                .OrderBy(t => t.SortOrder)
                .ToList();

            // Convert the townships to a SelectList for rendering in the dropdown
            var townshipList = townships.Select(t => new SelectListItem
            {
                Value = t.ID, // Assuming your Township model has the ID property as a string
                Text = t.Name // Assuming your Township model has the Name property
            });

            return Json(townshipList, JsonRequestBehavior.AllowGet);
        }

        // GET: Suppliers/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            Supplier supplier = db.Suppliers.Find(id);
            supplier.DeletedUserId = currentUser.Id;
            supplier.IsDeleted = true;
            supplier.DeletedDate = DateTime.Now;
            db.Entry(supplier).State = EntityState.Modified;
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
