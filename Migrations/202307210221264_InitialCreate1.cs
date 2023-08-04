namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.City", "CreatedUserId", c => c.String());
            AlterColumn("dbo.State", "CreatedUserId", c => c.String());
            AlterColumn("dbo.Township", "CreatedUserId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Township", "CreatedUserId", c => c.String(nullable: false));
            AlterColumn("dbo.State", "CreatedUserId", c => c.String(nullable: false));
            AlterColumn("dbo.City", "CreatedUserId", c => c.String(nullable: false));
        }
    }
}
