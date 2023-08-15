namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateStockSale : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StockSaleDetail", "CategoryId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.StockSaleDetail", "ProductId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.StockSaleDetail", "CategoryId");
            CreateIndex("dbo.StockSaleDetail", "ProductId");
            AddForeignKey("dbo.StockSaleDetail", "CategoryId", "dbo.Category", "ID");
            AddForeignKey("dbo.StockSaleDetail", "ProductId", "dbo.Product", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StockSaleDetail", "ProductId", "dbo.Product");
            DropForeignKey("dbo.StockSaleDetail", "CategoryId", "dbo.Category");
            DropIndex("dbo.StockSaleDetail", new[] { "ProductId" });
            DropIndex("dbo.StockSaleDetail", new[] { "CategoryId" });
            AlterColumn("dbo.StockSaleDetail", "ProductId", c => c.String(nullable: false, maxLength: 36));
            AlterColumn("dbo.StockSaleDetail", "CategoryId", c => c.String(nullable: false, maxLength: 36));
        }
    }
}
