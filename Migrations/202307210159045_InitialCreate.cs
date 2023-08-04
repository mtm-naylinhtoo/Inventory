namespace Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.City",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        StateId = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(nullable: false),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.State", t => t.StateId, cascadeDelete: true)
                .Index(t => t.StateId);
            
            CreateTable(
                "dbo.State",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(nullable: false),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Township",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        StateId = c.String(maxLength: 128),
                        CityId = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(nullable: false),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.City", t => t.CityId, cascadeDelete: true)
                .ForeignKey("dbo.State", t => t.StateId)
                .Index(t => t.StateId)
                .Index(t => t.CityId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(),
                        Email = c.String(nullable: false),
                        HashedPassword = c.String(),
                        Password = c.String(nullable: false),
                        Mobile = c.String(),
                        DOB = c.DateTime(),
                        Role = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedUserId = c.String(),
                        UpdatedDate = c.DateTime(),
                        UpdatedUserId = c.String(),
                        DeletedDate = c.DateTime(),
                        DeletedUserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Township", "StateId", "dbo.State");
            DropForeignKey("dbo.Township", "CityId", "dbo.City");
            DropForeignKey("dbo.City", "StateId", "dbo.State");
            DropIndex("dbo.Township", new[] { "CityId" });
            DropIndex("dbo.Township", new[] { "StateId" });
            DropIndex("dbo.City", new[] { "StateId" });
            DropTable("dbo.User");
            DropTable("dbo.Township");
            DropTable("dbo.State");
            DropTable("dbo.City");
        }
    }
}
