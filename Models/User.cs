using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Xml.Linq;
using BCrypt.Net;

namespace Inventory.Models
{
    public class User
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        // Plain text password for registration (not stored in the database)
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Password setter with automatic hashing
        public void SetPassword(string password)
        {
            this.Password = password;
            this.HashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        }

        [Display(Name = "Mobile")]
        public string Mobile { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        public int Role { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created User ID")]
        public string CreatedUserId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "Updated User ID")]
        public string UpdatedUserId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Deleted Date")]
        public DateTime? DeletedDate { get; set; }

        [Display(Name = "Deleted User ID")]
        public string DeletedUserId { get; set; }
    }
}