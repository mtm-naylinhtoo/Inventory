using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class StockPurchase
    {
        public string ID { get; set; } // Primary Key

        [MaxLength(50)]
        [Required]
        public string StockPurchaseNo { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(36)]
        [Required]
        public string SupplierId { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public int? DiscountPercent { get; set; }

        public decimal? DiscountAmount { get; set; }

        [Required]
        public decimal NetAmount { get; set; }

        public string Remark { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedUserId { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUserId { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedUserId { get; set; }

        public virtual ICollection<StockPurchaseDetail> StockPurchaseDetails { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}