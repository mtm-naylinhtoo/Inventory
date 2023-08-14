using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
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
    public class StockPurchasesController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: StockPurchases
        [UserAuthorizationFilter]
        public ActionResult Index(string search, bool? showDeleted, int? page)
        {
            var currentUser = (User)Session["CurrentUser"];
            ViewBag.currentUser = currentUser;

            var stockPurchases = db.StockPurchases.Include(sp => sp.Supplier);

            if (!string.IsNullOrEmpty(search))
            {
                stockPurchases = stockPurchases.Where(sp => sp.StockPurchaseNo.Contains(search) || sp.Supplier.Name.Contains(search));
            }

            if (showDeleted.HasValue && showDeleted.Value)
            {
                // If the checkbox is checked, do not filter by IsDeleted, show all stock purchases.
            }
            else
            {
                stockPurchases = stockPurchases.Where(sp => !sp.IsDeleted);
            }

            // Order by the CreatedDate column in descending order
            stockPurchases = stockPurchases.OrderByDescending(sp => sp.CreatedDate);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedStockPurchases = stockPurchases.ToPagedList(pageNumber, pageSize);

            return View(pagedStockPurchases);
        }


        // GET: StockPurchases/Details/5
        [UserAuthorizationFilter]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var stockPurchase = db.StockPurchases.Include(sp => sp.StockPurchaseDetails)
                                                  .SingleOrDefault(sp => sp.ID == id);

            if (stockPurchase == null)
            {
                return HttpNotFound();
            }

            var viewModel = new StockPurchaseDetailViewModel
            {
                StockPurchase = stockPurchase,
                CreatedUserName = GetUserNameById(stockPurchase.CreatedUserId),
                UpdatedUserName = GetUserNameById(stockPurchase.UpdatedUserId),
                DeletedUserName = GetUserNameById(stockPurchase.DeletedUserId),
                StockPurchaseDetails = stockPurchase.StockPurchaseDetails.ToList()
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

        [UserAuthorizationFilter]
        public ActionResult Create()
        {
            var viewModel = new StockPurchaseCreateViewModel
            {
                Products = new List<StockPurchaseProductViewModel>
        {
            new StockPurchaseProductViewModel() // Initial row
        }
            };

            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var suppliers = db.Suppliers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Suppliers = new SelectList(suppliers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Create(StockPurchaseCreateViewModel viewModel)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var codePrefix = "P";
                        var serialNo = 1;
                        int Month = DateTime.Now.Month;
                        int Year = DateTime.Now.Year;
                        AutoGenerate lastAutoGenerate = db.AutoGenerates
                                                        .Where(ag => ag.CodePrefix == "P")
                                                        .OrderByDescending(ag => ag.SerialNo)
                                                        .FirstOrDefault();
                        if (lastAutoGenerate != null)
                        {
                            serialNo = lastAutoGenerate.SerialNo + 1;
                        }
                        var ItemNo = $"P{Year}{Month:D2}{serialNo.ToString("D5")}";
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
                        var stockPurchase = new StockPurchase
                        {
                            ID = Guid.NewGuid().ToString(),
                            StockPurchaseNo = ItemNo,
                            Date = DateTime.Now,
                            SupplierId = viewModel.SupplierId,
                            TotalAmount = viewModel.TotalAmount,
                            DiscountPercent = viewModel.DiscountPercent,
                            DiscountAmount = viewModel.DiscountAmount,
                            NetAmount = viewModel.NetAmount,
                            Remark = viewModel.Remark,
                            CreatedDate = DateTime.Now,
                            CreatedUserId = currentUser.Id
                        };
                        db.StockPurchases.Add(stockPurchase);
                        db.SaveChanges();

                        foreach (var detail in viewModel.Products)
                        {
                            var stockPurchaseDetail = new StockPurchaseDetail
                            {
                                ID = Guid.NewGuid().ToString(),
                                StockPurchaseId = stockPurchase.ID,
                                CategoryId = detail.CategoryId,
                                ProductId = detail.ProductId,
                                Quantity = detail.Quantity,
                                Price = detail.Price,
                                Amount = detail.Amount,
                                CreatedDate = DateTime.Now,
                                CreatedUserId = currentUser.Id
                            };
                            db.StockPurchaseDetails.Add(stockPurchaseDetail);

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
                                    Balance = detail.Quantity,
                                    CreatedDate = DateTime.Now,
                                    CreatedUserId = currentUser.Id
                                };
                                db.StockBalances.Add(stockBalance);
                            }
                            else
                            {
                                stockBalance.Balance += detail.Quantity;
                                stockBalance.UpdatedDate = DateTime.Now;
                                stockBalance.UpdatedUserId = currentUser.Id;
                            }
                        }
                        db.SaveChanges();

                        transaction.Commit();
                        return RedirectToAction("Index", "StockPurchases");
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
            var suppliers = db.Suppliers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Suppliers = new SelectList(suppliers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");
            return View(viewModel);
        }


        // GET: StockPurchases/Edit/5
        [UserAuthorizationFilter]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var stockPurchase = db.StockPurchases
                .Include(sp => sp.StockPurchaseDetails)
                .SingleOrDefault(sp => sp.ID == id);

            if (stockPurchase == null)
            {
                return HttpNotFound();
            }

            var viewModel = new StockPurchaseEditViewModel
            {
                StockPurchase = stockPurchase,
                StockPurchaseId = stockPurchase.ID,
                SupplierId = stockPurchase.SupplierId,
                TotalAmount = stockPurchase.TotalAmount,
                DiscountPercent = stockPurchase.DiscountPercent,
                DiscountAmount = stockPurchase.DiscountAmount,
                NetAmount = stockPurchase.NetAmount,
                Remark = stockPurchase.Remark,
                Products = stockPurchase.StockPurchaseDetails.Select(detail => new StockPurchaseProductViewModel
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
            var suppliers = db.Suppliers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Suppliers = new SelectList(suppliers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorizationFilter]
        public ActionResult Edit(StockPurchaseEditViewModel viewModel)
        {
            var currentUser = (User)Session["CurrentUser"];
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var stockPurchase = db.StockPurchases
                            .Include(sp => sp.StockPurchaseDetails)
                            .SingleOrDefault(sp => sp.ID == viewModel.StockPurchaseId);

                        if (stockPurchase == null)
                        {
                            return HttpNotFound();
                        }

                        stockPurchase.SupplierId = viewModel.SupplierId;
                        stockPurchase.TotalAmount = viewModel.TotalAmount;
                        stockPurchase.DiscountPercent = viewModel.DiscountPercent;
                        stockPurchase.DiscountAmount = viewModel.DiscountAmount;
                        stockPurchase.NetAmount = viewModel.NetAmount;
                        stockPurchase.Remark = viewModel.Remark;
                        stockPurchase.UpdatedDate = DateTime.Now;
                        stockPurchase.UpdatedUserId = currentUser.Id;

                        // Update StockPurchaseDetails
                        foreach (var detail in viewModel.Products)
                        {
                            var existingDetail = stockPurchase.StockPurchaseDetails
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
                                        Balance = detail.Quantity,
                                        CreatedDate = DateTime.Now,
                                        CreatedUserId = currentUser.Id
                                    };
                                    db.StockBalances.Add(stockBalance);
                                }
                                else
                                {
                                    var balanceToUpdate = detail.Quantity - existingDetail.Quantity;
                                    stockBalance.Balance += balanceToUpdate;
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
                                var newDetail = new StockPurchaseDetail
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    StockPurchaseId = stockPurchase.ID,
                                    CategoryId = detail.CategoryId,
                                    ProductId = detail.ProductId,
                                    Quantity = detail.Quantity,
                                    Price = detail.Price,
                                    Amount = detail.Amount,
                                    CreatedDate = DateTime.Now,
                                    CreatedUserId = currentUser.Id
                                };
                                db.StockPurchaseDetails.Add(newDetail);

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
                                        Balance = detail.Quantity,
                                        CreatedDate = DateTime.Now,
                                        CreatedUserId = currentUser.Id
                                    };
                                    db.StockBalances.Add(stockBalance);
                                }
                                else
                                {
                                    stockBalance.Balance += detail.Quantity;
                                    stockBalance.UpdatedDate = DateTime.Now;
                                    stockBalance.UpdatedUserId = currentUser.Id;
                                }
                            }
                        }
                        // Remove existing details that are not included in the viewModel.Products
                        var existingDetailsToRemove = stockPurchase.StockPurchaseDetails
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
                                stockBalance.Balance -= detailToRemove.Quantity;
                                stockBalance.UpdatedDate = DateTime.Now;
                                stockBalance.UpdatedUserId = currentUser.Id;
                            }

                            db.StockPurchaseDetails.Remove(detailToRemove);
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        return RedirectToAction("Index", "StockPurchases");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            var test = ModelState.Values;
            var categories = db.Categories.Where(c => !c.IsDeleted).ToList();
            var products = db.Products.Where(p => !p.IsDeleted).ToList();
            var suppliers = db.Suppliers.Where(p => !p.IsDeleted).ToList();
            ViewBag.Suppliers = new SelectList(suppliers, "ID", "Name");
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            ViewBag.Products = new SelectList(products, "ID", "Name");
            return View(viewModel);
        }

        // POST: StockPurchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var currentUser = (User)Session["CurrentUser"];

                    // Find the StockPurchase
                    var stockPurchase = db.StockPurchases
                        .Include(sp => sp.StockPurchaseDetails)
                        .SingleOrDefault(sp => sp.ID == id);

                    if (stockPurchase == null)
                    {
                        return HttpNotFound();
                    }

                    // Update StockPurchase
                    stockPurchase.IsDeleted = true;
                    stockPurchase.DeletedDate = DateTime.Now;
                    stockPurchase.DeletedUserId = currentUser.Id;

                    // Update StockPurchaseDetails and adjust stock balance
                    foreach (var detail in stockPurchase.StockPurchaseDetails)
                    {
                        // Find the corresponding stock balance
                        var stockBalance = db.StockBalances
                            .SingleOrDefault(sb => sb.CategoryId == detail.CategoryId && sb.ProductId == detail.ProductId);

                        if (stockBalance != null)
                        {
                            stockBalance.Balance -= detail.Quantity;
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
