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
    public class StatesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: States
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var states = db.States.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                states = states.Where(s => s.Code.Contains(search) || s.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all states.
            }
            else
            {
                states = states.Where(s => !s.IsDeleted);
            }

            // Order by the SortOrder column in ascending order
            states = states.OrderBy(s => s.SortOrder);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedStates = states.ToPagedList(pageNumber, pageSize);

            return View(pagedStates);
        }

        // GET: States/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            State state = db.States.Find(id);
            if (state == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(state.CreatedUserId);
            string updatedUserName = GetUserNameById(state.UpdatedUserId);
            string deletedUserName = GetUserNameById(state.DeletedUserId);

            var viewModel = new StateDetailViewModel
            {
                State = state,
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

        // GET: States/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            return View();
        }

        // POST: States/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] State state)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                state.ID = Guid.NewGuid().ToString();
                state.IsActive = true;
                state.CreatedDate = DateTime.Now;
                state.CreatedUserId = currentUser.Id;
                db.States.Add(state);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(state);
        }

        // GET: States/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            State state = db.States.Find(id);
            if (state == null)
            {
                return HttpNotFound();
            }
            return View(state);
        }

        // POST: States/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,Code,Name,SortOrder,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] State state)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                state.UpdatedDate = DateTime.Now;
                state.UpdatedUserId = currentUser.Id;
                db.Entry(state).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(state);
        }

        // GET: States/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            State state = db.States.Find(id);
            if (state == null)
            {
                return HttpNotFound();
            }
            return View(state);
        }

        // POST: States/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            State state = db.States.Find(id);
            state.DeletedUserId = currentUser.Id;
            state.IsDeleted = true;
            state.DeletedDate = DateTime.Now;
            db.Entry(state).State = EntityState.Modified;
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
