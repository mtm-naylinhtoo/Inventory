namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryIdInProduct : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Product", "Category_ID", "dbo.Category");
            DropIndex("dbo.Product", new[] { "Category_ID" });
            RenameColumn(table: "dbo.Product", name: "Category_ID", newName: "CategoryId");
            AlterColumn("dbo.Product", "CategoryId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Product", "CategoryId");
            AddForeignKey("dbo.Product", "CategoryId", "dbo.Category", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Product", "CategoryId", "dbo.Category");
            DropIndex("dbo.Product", new[] { "CategoryId" });
            AlterColumn("dbo.Product", "CategoryId", c => c.String(maxLength: 128));
            RenameColumn(table: "dbo.Product", name: "CategoryId", newName: "Category_ID");
            CreateIndex("dbo.Product", "Category_ID");
            AddForeignKey("dbo.Product", "Category_ID", "dbo.Category", "ID");
        }
    }
}
