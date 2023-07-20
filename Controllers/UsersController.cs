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
using Inventory.ViewModels;
using Inventory.Mailers;
using System.Threading.Tasks;
using PagedList;
using PagedList.Mvc;

namespace Inventory.Controllers
{
    public class UsersController : Controller
    {
        private readonly EmailService _emailService;
        private InventoryContext db = new InventoryContext();
        public UsersController()
        {
            var mailHogHost = "localhost";
            var mailHogPort = 1025; // The default MailHog SMTP port
            EmailService emailService = new EmailService(mailHogHost, mailHogPort);
            _emailService = (EmailService)emailService;
        }

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

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Id,FirstName,LastName,Email,HashedPassword,Password,Mobile,DOB,Role,IsActive,IsDeleted,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId,DeletedDate,DeletedUserId")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Id = Guid.NewGuid().ToString();
                user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedDate = DateTime.Now;
                user.CreatedUserId = "Self Registered";
                user.Role = 0;
                user.IsActive = true;
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }
        public ActionResult Logout()
        {
            Session["CurrentUser"] = null;
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User currentUser = Session["CurrentUser"] as User;

                if (currentUser != null && BCrypt.Net.BCrypt.Verify(model.OldPassword, currentUser.HashedPassword))
                {
                    currentUser.HashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    currentUser.Password = model.NewPassword;
                    db.Entry(currentUser).State = EntityState.Modified;
                    db.SaveChanges();

                    ViewBag.SuccessMessage = "Password changed successfully!";
                    return View();
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid old password.";
                }
            }

            return View(model);
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            User user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var token = user.HashedPassword;
                var resetUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}/Users/ResetPassword?email={HttpUtility.UrlEncode(email)}&token={HttpUtility.UrlEncode(token)}";

                await _emailService.SendPasswordResetEmail(email, token, resetUrl);
                ViewBag.Message = "Email Sent!";
                return View();
            }
            ViewBag.Message = "Email not found";
            return View();

        }
        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            User user = db.Users.Find(token);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(new ResetPasswordViewModel { Token = token });

        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            User user = db.Users.Find(model.Token);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            if (model.NewPassword == model.ConfirmNewPassword)
            {
                user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.Password = model.NewPassword;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.Message = "Password reset successfully! You can now login with your new password.";
            }
            else
            {
                ViewBag.Message = "Password and Confirmed Password are not the same!";
            }
            return View(model);
        }
        // GET: Users
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var users = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all users.
            }
            else
            {
                users = users.Where(u => !u.IsDeleted);
            }
            users = users.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedUsers = users.ToPagedList(pageNumber, pageSize);

            return View(pagedUsers);

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
            var createdUser = db.Users.Find(user.CreatedUserId);
            var updatedUser = db.Users.Find(user.UpdatedUserId);
            var deletedUser = db.Users.Find(user.DeletedUserId);
            var viewModel = new UserDetailViewModel
            {
                User = user,
                CreatedUserName = createdUser != null ? $"{createdUser.FirstName} {createdUser.LastName}" : "Unknown User",
                UpdatedUserName = updatedUser != null ? $"{updatedUser.FirstName} {updatedUser.LastName}" : "Unknown User",
                DeletedUserName = deletedUser != null ? $"{deletedUser.FirstName} {deletedUser.LastName}" : "Unknown User"
            };
            return View(viewModel);
        }

        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            return View();
        }

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
                user.IsActive = true;
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
