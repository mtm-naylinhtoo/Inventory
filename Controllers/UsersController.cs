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
using BCrypt.Net;

namespace Inventory.Controllers
{
    public class UsersController : Controller
    {
        private InventoryContext db = new InventoryContext();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            User user = db.Users.Where(u => !u.IsDeleted).FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                Session["CurrentUser"] = user;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email or password";
                return View();
            }
        }
        // GET: Users
        [UserAuthorizationFilter]
        public ActionResult Index()
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;
            return View(db.Users.Where(u => !u.IsDeleted).ToList());
        }

        // GET: Users/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Email,HashedPassword,Password,Mobile,DOB,Role,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] User user)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {

                user.Id = Guid.NewGuid().ToString();
                user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedDate = DateTime.Now;
                user.CreatedUserId = currentUser.Id;
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit(User user)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                user.UpdatedDate = DateTime.Now;
                user.UpdatedUserId = currentUser.Id;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [UserAuthorizationFilter]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentUser = (User)Session["CurrentUser"];
            User user = db.Users.Find(id);
            user.DeletedUserId = currentUser.Id;
            user.IsDeleted = true;
            user.DeletedDate = DateTime.Now;
            db.Entry(user).State = EntityState.Modified;
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
