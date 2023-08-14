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
    public class StockBalancesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: StockBalances
        [UserAuthorizationFilter]
        public ActionResult Index(string search, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var stockBalances = db.StockBalances.Include(sb => sb.Category).Include(sb => sb.Product);

            if (!string.IsNullOrEmpty(search))
            {
                stockBalances = stockBalances.Where(sb => sb.Balance.ToString().Contains(search));
            }

            stockBalances = stockBalances.OrderBy(sb => sb.CreatedDate);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedStockBalances = stockBalances.ToPagedList(pageNumber, pageSize);

            return View(pagedStockBalances);
        }


        // GET: StockBalances/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StockBalance stockBalance = db.StockBalances.Find(id);
            if (stockBalance == null)
            {
                return HttpNotFound();
            }

            string createdUserName = GetUserNameById(stockBalance.CreatedUserId);
            string updatedUserName = GetUserNameById(stockBalance.UpdatedUserId);

            var viewModel = new StockBalanceDetailViewModel
            {
                StockBalance = stockBalance,
                CreatedUserName = createdUserName,
                UpdatedUserName = updatedUserName
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

        // GET: StockBalances/Create
        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();

            ViewBag.CategoryId = new SelectList(categories, "ID", "Name");
            ViewBag.ProductId = new SelectList(products, "ID", "Name");
            return View();
        }

        // POST: StockBalances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create([Bind(Include = "ID,CategoryId,ProductId,Balance,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId")] StockBalance stockBalance)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                stockBalance.ID = Guid.NewGuid().ToString();
                stockBalance.CreatedDate = DateTime.Now;
                stockBalance.CreatedUserId = currentUser.Id;
                db.StockBalances.Add(stockBalance);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", stockBalance.CategoryId);
            ViewBag.ProductId = new SelectList(products, "ID", "Name", stockBalance.ProductId);
            return View(stockBalance);
        }

        // GET: StockBalances/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StockBalance stockBalance = db.StockBalances.Find(id);
            if (stockBalance == null)
            {
                return HttpNotFound();
            }

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();

            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", stockBalance.CategoryId);
            ViewBag.ProductId = new SelectList(products, "ID", "Name", stockBalance.ProductId);
            return View(stockBalance);
        }

        // POST: StockBalances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit([Bind(Include = "ID,CategoryId,ProductId,Balance,CreatedDate,CreatedUserId,UpdatedDate,UpdatedUserId")] StockBalance stockBalance)
        {
            if (ModelState.IsValid)
            {
                var currentUser = (User)Session["CurrentUser"];
                stockBalance.UpdatedDate = DateTime.Now;
                stockBalance.UpdatedUserId = currentUser.Id;
                db.Entry(stockBalance).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            ViewBag.CategoryId = new SelectList(categories, "ID", "Name", stockBalance.CategoryId);
            ViewBag.ProductId = new SelectList(products, "ID", "Name", stockBalance.ProductId);
            return View(stockBalance);
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
