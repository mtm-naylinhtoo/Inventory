using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class StockBalance
    {
        public string ID { get; set; } // Primary Key
        [Required]
        public string CategoryId { get; set; }
        [Required]
        public string ProductId { get; set; }
        [Required]
        public decimal Balance { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedUserId { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUserId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Product Product { get; set; }
    }
}