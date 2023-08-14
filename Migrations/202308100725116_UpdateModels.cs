namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModels : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StockPurchaseDetail", "CategoryId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.StockPurchaseDetail", "ProductId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.StockPurchaseDetail", "CategoryId");
            CreateIndex("dbo.StockPurchaseDetail", "ProductId");
            AddForeignKey("dbo.StockPurchaseDetail", "CategoryId", "dbo.Category", "ID");
            AddForeignKey("dbo.StockPurchaseDetail", "ProductId", "dbo.Product", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StockPurchaseDetail", "ProductId", "dbo.Product");
            DropForeignKey("dbo.StockPurchaseDetail", "CategoryId", "dbo.Category");
            DropIndex("dbo.StockPurchaseDetail", new[] { "ProductId" });
            DropIndex("dbo.StockPurchaseDetail", new[] { "CategoryId" });
            AlterColumn("dbo.StockPurchaseDetail", "ProductId", c => c.String(nullable: false, maxLength: 36));
            AlterColumn("dbo.StockPurchaseDetail", "CategoryId", c => c.String(nullable: false, maxLength: 36));
        }
    }
}
