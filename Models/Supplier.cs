using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class Supplier
    {
        public string Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }

        [Required]
        public string StateId { get; set; }

        [Required]
        public string CityId { get; set; }

        [Required]
        public string TownshipId { get; set; }

        [Required]
        public string Address { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(36)]
        public string CreatedUserId { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(36)]
        public string UpdatedUserId { get; set; }

        public DateTime? DeletedDate { get; set; }

        [StringLength(36)]
        public string DeletedUserId { get; set; }

        // Navigation properties
        public virtual State State { get; set; }
        public virtual City City { get; set; }
        public virtual Township Township { get; set; }
    }
}