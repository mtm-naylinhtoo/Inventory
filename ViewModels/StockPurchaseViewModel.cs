using Inventory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory.ViewModels
{
    public class StockPurchaseProductViewModel
    {
        [Required]
        public string CategoryId { get; set; }

        [Required]
        public string ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public decimal Amount { get; set; }
    }

    public class StockPurchaseCreateViewModel
    {
        [Required]
        public string SupplierId { get; set; }

        public List<StockPurchaseProductViewModel> Products { get; set; }

        public decimal TotalAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        [Required]
        public decimal NetAmount { get; set; }
        public string Remark { get; set; }
    }
    public class StockPurchaseEditViewModel
    {
        public StockPurchase StockPurchase { get; set; } // Include the StockPurchase entity

        public string StockPurchaseId { get; set; }

        [Required]
        public string SupplierId { get; set; }

        public List<StockPurchaseProductViewModel> Products { get; set; }

        public decimal TotalAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
        public int? DiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be a positive value.")]
        public decimal? DiscountAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Net amount must be a positive value.")]
        public decimal NetAmount { get; set; }

        public string Remark { get; set; }
    }

    public class StockPurchaseDetailViewModel
    {
        public StockPurchase StockPurchase { get; set; }
        public string CreatedUserName { get; set; }
        public string UpdatedUserName { get; set; }
        public string DeletedUserName { get; set; }
        public List<StockPurchaseDetail> StockPurchaseDetails { get; set; }
    }
}
