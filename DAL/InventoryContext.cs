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
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Township> Townships { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SalePrice> SalePrices { get; set; }
        public DbSet<StockBalance> StockBalances { get; set; }
        public DbSet<AutoGenerate> AutoGenerates { get; set; }
        public DbSet<StockSale> StockSales { get; set; }
        public DbSet<StockSaleDetail> StockSaleDetails { get; set; }
        public DbSet<StockPurchase> StockPurchases { get; set; }
        public DbSet<StockPurchaseDetail> StockPurchaseDetails { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // for supplier

            modelBuilder.Entity<Supplier>()
                .HasRequired(s => s.State)
                .WithMany()
                .HasForeignKey(s => s.StateId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Supplier>()
                .HasRequired(s => s.City)
                .WithMany()
                .HasForeignKey(s => s.CityId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Supplier>()
                .HasRequired(s => s.Township)
                .WithMany()
                .HasForeignKey(s => s.TownshipId)
                .WillCascadeOnDelete(true);

            // for Customer

            modelBuilder.Entity<Customer>()
                .HasRequired(c => c.State)
                .WithMany()
                .HasForeignKey(c => c.StateId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Customer>()
                .HasRequired(c => c.City)
                .WithMany()
                .HasForeignKey(c => c.CityId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Customer>()
                .HasRequired(c => c.Township)
                .WithMany()
                .HasForeignKey(c => c.TownshipId)
                .WillCascadeOnDelete(true);

            // for SalePrice

            modelBuilder.Entity<SalePrice>()
                .HasRequired(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SalePrice>()
                .HasRequired(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .WillCascadeOnDelete(true);

            // for StockBalance

            modelBuilder.Entity<StockBalance>()
                .HasRequired(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockBalance>()
                .HasRequired(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .WillCascadeOnDelete(true);

            // for StockPurchaseDetail

            modelBuilder.Entity<StockPurchaseDetail>()
                .HasRequired(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockPurchaseDetail>()
                .HasRequired(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .WillCascadeOnDelete(true);
        }

        public System.Data.Entity.DbSet<Inventory.Models.Customer> Customers { get; set; }
    }
}