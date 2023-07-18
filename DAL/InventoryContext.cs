using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inventory.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Inventory.DAL
{
    public class InventoryContext : DbContext
    {
        public InventoryContext() : base("InventoryContext")
        {
        }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}