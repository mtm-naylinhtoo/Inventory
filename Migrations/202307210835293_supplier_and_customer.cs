namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class supplier_and_customer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customer",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        Mobile = c.String(nullable: false, maxLength: 20),
                        StateId = c.String(nullable: false, maxLength: 128),
                        CityId = c.String(nullable: false, maxLength: 128),
                        TownshipId = c.String(nullable: false, maxLength: 128),
                        Address = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(nullable: false, maxLength: 36),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(maxLength: 36),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(maxLength: 36),
                        State_ID = c.String(maxLength: 128),
                        Township_ID = c.String(maxLength: 128),
                        City_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.City", t => t.CityId)
                .ForeignKey("dbo.State", t => t.State_ID)
                .ForeignKey("dbo.Township", t => t.Township_ID)
                .ForeignKey("dbo.State", t => t.StateId)
                .ForeignKey("dbo.Township", t => t.TownshipId)
                .ForeignKey("dbo.City", t => t.City_ID)
                .Index(t => t.StateId)
                .Index(t => t.CityId)
                .Index(t => t.TownshipId)
                .Index(t => t.State_ID)
                .Index(t => t.Township_ID)
                .Index(t => t.City_ID);
            
            CreateTable(
                "dbo.Supplier",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        Mobile = c.String(nullable: false, maxLength: 20),
                        StateId = c.String(nullable: false, maxLength: 128),
                        CityId = c.String(nullable: false, maxLength: 128),
                        TownshipId = c.String(nullable: false, maxLength: 128),
                        Address = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(nullable: false, maxLength: 36),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(maxLength: 36),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(maxLength: 36),
                        Township_ID = c.String(maxLength: 128),
                        State_ID = c.String(maxLength: 128),
                        City_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.City", t => t.CityId)
                .ForeignKey("dbo.State", t => t.StateId)
                .ForeignKey("dbo.Township", t => t.Township_ID)
                .ForeignKey("dbo.Township", t => t.TownshipId, cascadeDelete: true)
                .ForeignKey("dbo.State", t => t.State_ID)
                .ForeignKey("dbo.City", t => t.City_ID)
                .Index(t => t.StateId)
                .Index(t => t.CityId)
                .Index(t => t.TownshipId)
                .Index(t => t.Township_ID)
                .Index(t => t.State_ID)
                .Index(t => t.City_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Supplier", "City_ID", "dbo.City");
            DropForeignKey("dbo.Customer", "City_ID", "dbo.City");
            DropForeignKey("dbo.Customer", "TownshipId", "dbo.Township");
            DropForeignKey("dbo.Customer", "StateId", "dbo.State");
            DropForeignKey("dbo.Supplier", "State_ID", "dbo.State");
            DropForeignKey("dbo.Supplier", "TownshipId", "dbo.Township");
            DropForeignKey("dbo.Supplier", "Township_ID", "dbo.Township");
            DropForeignKey("dbo.Customer", "Township_ID", "dbo.Township");
            DropForeignKey("dbo.Supplier", "StateId", "dbo.State");
            DropForeignKey("dbo.Supplier", "CityId", "dbo.City");
            DropForeignKey("dbo.Customer", "State_ID", "dbo.State");
            DropForeignKey("dbo.Customer", "CityId", "dbo.City");
            DropIndex("dbo.Supplier", new[] { "City_ID" });
            DropIndex("dbo.Supplier", new[] { "State_ID" });
            DropIndex("dbo.Supplier", new[] { "Township_ID" });
            DropIndex("dbo.Supplier", new[] { "TownshipId" });
            DropIndex("dbo.Supplier", new[] { "CityId" });
            DropIndex("dbo.Supplier", new[] { "StateId" });
            DropIndex("dbo.Customer", new[] { "City_ID" });
            DropIndex("dbo.Customer", new[] { "Township_ID" });
            DropIndex("dbo.Customer", new[] { "State_ID" });
            DropIndex("dbo.Customer", new[] { "TownshipId" });
            DropIndex("dbo.Customer", new[] { "CityId" });
            DropIndex("dbo.Customer", new[] { "StateId" });
            DropTable("dbo.Supplier");
            DropTable("dbo.Customer");
        }
    }
}
