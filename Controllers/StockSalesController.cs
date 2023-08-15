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
    public class StockSalesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: StockSales
        [UserAuthorizationFilter(CheckUserRole = false)]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var stockSales = db.StockSales.Include(ss => ss.Customer);

            if (!string.IsNullOrEmpty(search))
            {
                stockSales = stockSales.Where(ss => ss.StockSaleNo.Contains(search) || ss.Customer.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all stock sales.
            }
            else
            {
                stockSales = stockSales.Where(ss => !ss.IsDeleted);
            }

            // Order by the CreatedDate column in descending order
            stockSales = stockSales.OrderByDescending(ss => ss.CreatedDate);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedStockSales = stockSales.ToPagedList(pageNumber, pageSize);

            return View(pagedStockSales);
        }


        // GET: StockSales/Details/5
        [UserAuthorizationFilter(CheckUserRole = false)]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var stockSale = db.StockSales.Include(ss => ss.StockSaleDetails)
                                         .SingleOrDefault(ss => ss.ID == id);

            if (stockSale == null)
            {
                return HttpNotFound();
            }

            var viewModel = new StockSaleDetailViewModel
            {
                StockSale = stockSale,
                CreatedUserName = GetUserNameById(stockSale.CreatedUserId),
                UpdatedUserName = GetUserNameById(stockSale.UpdatedUserId),
                DeletedUserName = GetUserNameById(stockSale.DeletedUserId),
                StockSaleDetails = stockSale.StockSaleDetails.ToList()
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

        [UserAuthorizationFilter(CheckUserRole = false)]
        public ActionResult Create()
        {
            var viewModel = new StockSaleCreateViewModel
            {
                Products = new List<StockSaleProductViewModel>
        {
            new StockSaleProductViewModel() // Initial row
        }
            };

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var customers = db.Customers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Customers = new SelectList(customers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter(CheckUserRole = false)]
        public ActionResult Create(StockSaleCreateViewModel viewModel)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var codePrefix = "S";
                        var serialNo = 1;
                        int Month = DateTime.Now.Month;
                        int Year = DateTime.Now.Year;
                        AutoGenerate lastAutoGenerate = db.AutoGenerates
                                                        .Where(ag => ag.CodePrefix == "S")
                                                        .OrderByDescending(ag => ag.SerialNo)
                                                        .FirstOrDefault();
                        if (lastAutoGenerate != null)
                        {
                            serialNo = lastAutoGenerate.SerialNo + 1;
                        }
                        var ItemNo = $"S{Year}{Month:D2}{serialNo.ToString("D5")}";
                        var autoGenerate = new AutoGenerate
                        {
                            ID = Guid.NewGuid().ToString(),
                            ItemNo = ItemNo,
                            CodePrefix = codePrefix,
                            SerialNo = serialNo,
                            Month = Month,
                            Year = Year,
                            CreatedDate = DateTime.Now,
                            CreatedUserId = currentUser.Id
                        };
                        db.AutoGenerates.Add(autoGenerate);
                        db.SaveChanges();
                        var stockSale = new StockSale
                        {
                            ID = Guid.NewGuid().ToString(),
                            StockSaleNo = ItemNo,
                            Date = DateTime.Now,
                            CustomerId = viewModel.CustomerId,
                            TotalAmount = viewModel.TotalAmount,
                            DiscountPercent = viewModel.DiscountPercent,
                            DiscountAmount = viewModel.DiscountAmount,
                            NetAmount = viewModel.NetAmount,
                            Remark = viewModel.Remark,
                            CreatedDate = DateTime.Now,
                            CreatedUserId = currentUser.Id
                        };
                        db.StockSales.Add(stockSale);
                        db.SaveChanges();

                        foreach (var detail in viewModel.Products)
                        {
                            var stockSaleDetail = new StockSaleDetail
                            {
                                ID = Guid.NewGuid().ToString(),
                                StockSaleId = stockSale.ID,
                                CategoryId = detail.CategoryId,
                                ProductId = detail.ProductId,
                                Quantity = detail.Quantity,
                                Price = detail.Price,
                                Amount = detail.Amount,
                                CreatedDate = DateTime.Now,
                                CreatedUserId = currentUser.Id
                            };
                            db.StockSaleDetails.Add(stockSaleDetail);

                            //Update or create StockBalance entry
                            var stockBalance = db.StockBalances.FirstOrDefault(sb =>
                                sb.CategoryId == detail.CategoryId && sb.ProductId == detail.ProductId);
                            if (stockBalance == null)
                            {
                                stockBalance = new StockBalance
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    CategoryId = detail.CategoryId,
                                    ProductId = detail.ProductId,
                                    Balance = detail.Quantity * -1,
                                    CreatedDate = DateTime.Now,
                                    CreatedUserId = currentUser.Id
                                };
                                db.StockBalances.Add(stockBalance);
                            }
                            else
                            {
                                stockBalance.Balance -= detail.Quantity;
                                stockBalance.UpdatedDate = DateTime.Now;
                                stockBalance.UpdatedUserId = currentUser.Id;
                            }
                        }
                        db.SaveChanges();

                        transaction.Commit();
                        return RedirectToAction("Index", "StockSales");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var customers = db.Customers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Customers = new SelectList(customers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");
            return View(viewModel);
        }


        // GET: StockSales/Edit/5
        [UserAuthorizationFilter(CheckUserRole = false)]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var stockSale = db.StockSales
                .Include(sp => sp.StockSaleDetails)
                .SingleOrDefault(sp => sp.ID == id);

            if (stockSale == null)
            {
                return HttpNotFound();
            }

            var viewModel = new StockSaleEditViewModel
            {
                StockSale = stockSale,
                StockSaleId = stockSale.ID,
                CustomerId = stockSale.CustomerId,
                TotalAmount = stockSale.TotalAmount,
                DiscountPercent = stockSale.DiscountPercent,
                DiscountAmount = stockSale.DiscountAmount,
                NetAmount = stockSale.NetAmount,
                Remark = stockSale.Remark,
                Products = stockSale.StockSaleDetails.Select(detail => new StockSaleProductViewModel
                {
                    CategoryId = detail.CategoryId,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    Amount = detail.Amount
                }).ToList()
            };

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var customers = db.Customers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Customers = new SelectList(customers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter(CheckUserRole = false)]
        public ActionResult Edit(StockSaleEditViewModel viewModel)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var stockSale = db.StockSales
                            .Include(sp => sp.StockSaleDetails)
                            .SingleOrDefault(sp => sp.ID == viewModel.StockSaleId);

                        if (stockSale == null)
                        {
                            return HttpNotFound();
                        }

                        stockSale.CustomerId = viewModel.CustomerId;
                        stockSale.TotalAmount = viewModel.TotalAmount;
                        stockSale.DiscountPercent = viewModel.DiscountPercent;
                        stockSale.DiscountAmount = viewModel.DiscountAmount;
                        stockSale.NetAmount = viewModel.NetAmount;
                        stockSale.Remark = viewModel.Remark;
                        stockSale.UpdatedDate = DateTime.Now;
                        stockSale.UpdatedUserId = currentUser.Id;

                        // Update StockSaleDetails
                        foreach (var detail in viewModel.Products)
                        {
                            var existingDetail = stockSale.StockSaleDetails
                                .SingleOrDefault(d => d.CategoryId == detail.CategoryId && d.ProductId == detail.ProductId);

                            if (existingDetail != null)
                            {
                                //Update or create StockBalance entry
                                var stockBalance = db.StockBalances.FirstOrDefault(sb =>
                                    sb.CategoryId == detail.CategoryId && sb.ProductId == detail.ProductId);
                                if (stockBalance == null)
                                {
                                    stockBalance = new StockBalance
                                    {
                                        ID = Guid.NewGuid().ToString(),
                                        CategoryId = detail.CategoryId,
                                        ProductId = detail.ProductId,
                                        Balance = detail.Quantity * -1,
                                        CreatedDate = DateTime.Now,
                                        CreatedUserId = currentUser.Id
                                    };
                                    db.StockBalances.Add(stockBalance);
                                }
                                else
                                {
                                    var balanceToUpdate = detail.Quantity - existingDetail.Quantity;
                                    stockBalance.Balance -= balanceToUpdate;
                                    stockBalance.UpdatedDate = DateTime.Now;
                                    stockBalance.UpdatedUserId = currentUser.Id;
                                }

                                // update the actual detail
                                existingDetail.Quantity = detail.Quantity;
                                existingDetail.Price = detail.Price;
                                existingDetail.Amount = detail.Amount;
                                existingDetail.UpdatedDate = DateTime.Now;
                                existingDetail.UpdatedUserId = currentUser.Id;

                            }
                            else
                            {
                                var newDetail = new StockSaleDetail
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    StockSaleId = stockSale.ID,
                                    CategoryId = detail.CategoryId,
                                    ProductId = detail.ProductId,
                                    Quantity = detail.Quantity,
                                    Price = detail.Price,
                                    Amount = detail.Amount,
                                    CreatedDate = DateTime.Now,
                                    CreatedUserId = currentUser.Id
                                };
                                db.StockSaleDetails.Add(newDetail);

                                //Update or create StockBalance entry
                                var stockBalance = db.StockBalances.FirstOrDefault(sb =>
                                    sb.CategoryId == detail.CategoryId && sb.ProductId == detail.ProductId);
                                if (stockBalance == null)
                                {
                                    stockBalance = new StockBalance
                                    {
                                        ID = Guid.NewGuid().ToString(),
                                        CategoryId = detail.CategoryId,
                                        ProductId = detail.ProductId,
                                        Balance = detail.Quantity * -1,
                                        CreatedDate = DateTime.Now,
                                        CreatedUserId = currentUser.Id
                                    };
                                    db.StockBalances.Add(stockBalance);
                                }
                                else
                                {
                                    stockBalance.Balance -= detail.Quantity;
                                    stockBalance.UpdatedDate = DateTime.Now;
                                    stockBalance.UpdatedUserId = currentUser.Id;
                                }
                            }
                        }
                        // Remove existing details that are not included in the viewModel.Products
                        var existingDetailsToRemove = stockSale.StockSaleDetails
                            .Where(existingDetail => !viewModel.Products.Any(detail =>
                                detail.CategoryId == existingDetail.CategoryId && detail.ProductId == existingDetail.ProductId))
                            .ToList();

                        foreach (var detailToRemove in existingDetailsToRemove)
                        {

                            //Update or create StockBalance entry
                            var stockBalance = db.StockBalances.FirstOrDefault(sb =>
                                sb.CategoryId == detailToRemove.CategoryId && sb.ProductId == detailToRemove.ProductId);
                            if (stockBalance != null)
                            {
                                stockBalance.Balance += detailToRemove.Quantity;
                                stockBalance.UpdatedDate = DateTime.Now;
                                stockBalance.UpdatedUserId = currentUser.Id;
                            }

                            db.StockSaleDetails.Remove(detailToRemove);
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        return RedirectToAction("Index", "StockSales");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var customers = db.Customers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Customers = new SelectList(customers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");
            return View(viewModel);
        }

        // POST: StockSales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var currentUser = (User)Session["CurrentUser"];

                    // Find the StockSale
                    var stockSale = db.StockSales
                        .Include(sp => sp.StockSaleDetails)
                        .SingleOrDefault(sp => sp.ID == id);

                    if (stockSale == null)
                    {
                        return HttpNotFound();
                    }

                    // Update StockSale
                    stockSale.IsDeleted = true;
                    stockSale.DeletedDate = DateTime.Now;
                    stockSale.DeletedUserId = currentUser.Id;

                    // Update StockSaleDetails and adjust stock balance
                    foreach (var detail in stockSale.StockSaleDetails)
                    {
                        // Find the corresponding stock balance
                        var stockBalance = db.StockBalances
                            .SingleOrDefault(sb => sb.CategoryId == detail.CategoryId && sb.ProductId == detail.ProductId);

                        if (stockBalance != null)
                        {
                            stockBalance.Balance += detail.Quantity;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
            }
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

        public ActionResult GetSalePriceByCategoryAndProduct(string categoryId, string productId)
        {
            decimal price = 0; // Default price if not found

            // Retrieve the corresponding sale price from the SalePrice table
            var salePrice = db.SalePrices
                .FirstOrDefault(sp => sp.CategoryId == categoryId && sp.ProductId == productId);

            if (salePrice != null && decimal.TryParse(salePrice.Price, out decimal parsedPrice))
            {
                price = parsedPrice;
            }

            return Json(price, JsonRequestBehavior.AllowGet);
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
