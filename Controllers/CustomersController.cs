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
    public class CustomersController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Customers
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var customers = db.Customers.Include(c => c.City).Include(c => c.State).Include(c => c.Township);

            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c => c.Code.Contains(search) || c.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all customers.
            }
            else
            {
                customers = customers.Where(c => !c.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            customers = customers.OrderBy(c => c.Name);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedCustomers = customers.ToPagedList(pageNumber, pageSize);

            return View(pagedCustomers);
        }

        // GET: Customers/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(customer.CreatedUserId);
            string updatedUserName = GetUserNameById(customer.UpdatedUserId);
            string deletedUserName = GetUserNameById(customer.DeletedUserId);

            var viewModel = new CustomerDetailViewModel
            {
                Customer = customer,
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

        // GET: Customers/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.UserId = currentUser.Id;
            ViewBag.CreatedDate = DateTime.Now;
            ViewBag.StateId = new SelectList(db.States, "ID", "Name");
            ViewBag.CityId = new SelectList(Enumerable.Empty<City>(), "ID", "Name");
            ViewBag.TownshipId = new SelectList(Enumerable.Empty<Township>(), "ID", "Name");
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,StateId,CityId,TownshipId,Code,Name,Mobile,Address,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Customer customer)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                customer.Id = Guid.NewGuid().ToString();
                customer.IsActive = true;
                customer.CreatedDate = DateTime.Now;
                customer.CreatedUserId = currentUser.Id;
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States, "ID", "Name", customer.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(c => c.StateId == customer.StateId), "ID", "Name", customer.CityId);
            ViewBag.TownshipId = new SelectList(db.Townships.Where(t => t.CityId == customer.CityId), "ID", "Name", customer.TownshipId);
            return View(customer);
        }

        // GET: Customers/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            ViewBag.StateId = new SelectList(db.States, "ID", "Name", customer.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(c => c.StateId == customer.StateId), "ID", "Name", customer.CityId);
            ViewBag.TownshipId = new SelectList(db.Townships.Where(t => t.CityId == customer.CityId), "ID", "Name", customer.TownshipId);
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,StateId,CityId,TownshipId,Code,Name,Mobile,Address,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                customer.UpdatedDate = DateTime.Now;
                customer.UpdatedUserId = currentUser.Id;
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States, "ID", "Name", customer.StateId);
            ViewBag.CityId = new SelectList(db.Cities.Where(c => c.StateId == customer.StateId), "ID", "Name", customer.CityId);
            ViewBag.TownshipId = new SelectList(db.Townships.Where(t => t.CityId == customer.CityId), "ID", "Name", customer.TownshipId);
            return View(customer);
        }

        // GET: Customers/GetCitiesByState
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
            var cities = db.Cities
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

        // GET: Customers/GetTownshipsByCity
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
            var townships = db.Townships
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

        // GET: Customers/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            Customer customer = db.Customers.Find(id);
            customer.DeletedUserId = currentUser.Id;
            customer.IsDeleted = true;
            customer.DeletedDate = DateTime.Now;
            db.Entry(customer).State = EntityState.Modified;
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
