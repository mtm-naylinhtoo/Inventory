using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class SalePrice
    {
        public string ID { get; set; } // Primary Key
        [Required]
        public string CategoryId { get; set; }
        [Required]
        public string ProductId { get; set; }
        [Required]
        [StringLength(50)]
        public string Price { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedUserId { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUserId { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedUserId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Product Product { get; set; }
    }
}