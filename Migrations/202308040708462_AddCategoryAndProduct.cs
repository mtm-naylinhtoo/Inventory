namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryAndProduct : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Product",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                        Category_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Category", t => t.Category_ID)
                .Index(t => t.Category_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Product", "Category_ID", "dbo.Category");
            DropIndex("dbo.Product", new[] { "Category_ID" });
            DropTable("dbo.Product");
            DropTable("dbo.Category");
        }
    }
}
