namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSalePrice : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Customer", "TownshipId", "dbo.Township");
            CreateTable(
                "dbo.SalePrice",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(nullable: false, maxLength: 128),
                        Price = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.ProductId);
            
            AddForeignKey("dbo.Customer", "TownshipId", "dbo.Township", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Customer", "TownshipId", "dbo.Township");
            DropForeignKey("dbo.SalePrice", "ProductId", "dbo.Product");
            DropForeignKey("dbo.SalePrice", "CategoryId", "dbo.Category");
            DropIndex("dbo.SalePrice", new[] { "ProductId" });
            DropIndex("dbo.SalePrice", new[] { "CategoryId" });
            DropTable("dbo.SalePrice");
            AddForeignKey("dbo.Customer", "TownshipId", "dbo.Township", "ID");
        }
    }
}
