using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BCrypt.Net;

namespace Inventory.DAL
{
    public class InventoryInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<InventoryContext>
    {
        protected override void Seed(InventoryContext context)
        {
            var users = new List<User>
            {
                new User{
                Id = Guid.NewGuid().ToString(),
                FirstName = "Nay",
                LastName = "Nay",
                Email = "naynay@example.com",
                HashedPassword = BCrypt.Net.BCrypt.HashPassword("password1"),
                Role = 0,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                CreatedUserId = Guid.NewGuid().ToString(),
                UpdatedDate = null,
                UpdatedUserId = null,
                DeletedDate = null,
                DeletedUserId = null},
                new User{
                Id = Guid.NewGuid().ToString(),
                FirstName = "Lin",
                LastName = "Lin",
                Email = "linlin@example.com",
                HashedPassword = BCrypt.Net.BCrypt.HashPassword("password1"),
                Role = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                CreatedUserId = Guid.NewGuid().ToString(),
                UpdatedDate = null,
                UpdatedUserId = null,
                DeletedDate = null,
                DeletedUserId = null},
            };

            users.ForEach(s => context.Users.Add(s));
            context.SaveChanges();
        }
    }
}