using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class AutoGenerate
    {
        public string ID { get; set; } // Primary Key

        [MaxLength(50)]
        [Required]
        public string ItemNo { get; set; }

        [MaxLength(10)]
        [Required]
        public string CodePrefix { get; set; }

        [Required]
        public int SerialNo { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedUserId { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUserId { get; set; }
    }
}