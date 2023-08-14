namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStocks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AutoGenerate",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        ItemNo = c.String(nullable: false, maxLength: 50),
                        CodePrefix = c.String(nullable: false, maxLength: 10),
                        SerialNo = c.Int(nullable: false),
                        Month = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.StockBalance",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(nullable: false, maxLength: 128),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.StockPurchaseDetail",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        StockPurchaseId = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(nullable: false, maxLength: 36),
                        ProductId = c.String(nullable: false, maxLength: 36),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.StockPurchase", t => t.StockPurchaseId, cascadeDelete: true)
                .Index(t => t.StockPurchaseId);
            
            CreateTable(
                "dbo.StockPurchase",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        StockPurchaseNo = c.String(nullable: false, maxLength: 50),
                        Date = c.DateTime(nullable: false),
                        SupplierId = c.String(nullable: false, maxLength: 128),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercent = c.Int(),
                        DiscountAmount = c.Decimal(precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remark = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Supplier", t => t.SupplierId, cascadeDelete: true)
                .Index(t => t.SupplierId);
            
            CreateTable(
                "dbo.StockSaleDetail",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        StockSaleId = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(nullable: false, maxLength: 36),
                        ProductId = c.String(nullable: false, maxLength: 36),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.StockSale", t => t.StockSaleId, cascadeDelete: true)
                .Index(t => t.StockSaleId);
            
            CreateTable(
                "dbo.StockSale",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        StockSaleNo = c.String(nullable: false, maxLength: 50),
                        Date = c.DateTime(nullable: false),
                        CustomerId = c.String(nullable: false, maxLength: 128),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercent = c.Int(),
                        DiscountAmount = c.Decimal(precision: 18, scale: 2),
                        NetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Remark = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StockSaleDetail", "StockSaleId", "dbo.StockSale");
            DropForeignKey("dbo.StockSale", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.StockPurchaseDetail", "StockPurchaseId", "dbo.StockPurchase");
            DropForeignKey("dbo.StockPurchase", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.StockBalance", "ProductId", "dbo.Product");
            DropForeignKey("dbo.StockBalance", "CategoryId", "dbo.Category");
            DropIndex("dbo.StockSale", new[] { "CustomerId" });
            DropIndex("dbo.StockSaleDetail", new[] { "StockSaleId" });
            DropIndex("dbo.StockPurchase", new[] { "SupplierId" });
            DropIndex("dbo.StockPurchaseDetail", new[] { "StockPurchaseId" });
            DropIndex("dbo.StockBalance", new[] { "ProductId" });
            DropIndex("dbo.StockBalance", new[] { "CategoryId" });
            DropTable("dbo.StockSale");
            DropTable("dbo.StockSaleDetail");
            DropTable("dbo.StockPurchase");
            DropTable("dbo.StockPurchaseDetail");
            DropTable("dbo.StockBalance");
            DropTable("dbo.AutoGenerate");
        }
    }
}
