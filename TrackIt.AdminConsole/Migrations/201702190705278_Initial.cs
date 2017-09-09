namespace TrackIt.AdminConsole.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AccountId = c.String(),
                        Description = c.String(nullable: false, maxLength: 128),
                        Units = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataPoints",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        Stamp = c.DateTime(nullable: false),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataPoints", "CategoryId", "dbo.Categories");
            DropIndex("dbo.DataPoints", new[] { "CategoryId" });
            DropTable("dbo.DataPoints");
            DropTable("dbo.Categories");
        }
    }
}
