namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customer", "CreatedUserId", c => c.String(maxLength: 36));
            AlterColumn("dbo.Supplier", "CreatedUserId", c => c.String(maxLength: 36));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Supplier", "CreatedUserId", c => c.String(nullable: false, maxLength: 36));
            AlterColumn("dbo.Customer", "CreatedUserId", c => c.String(nullable: false, maxLength: 36));
        }
    }
}
