

namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddProductFields : DbMigration
    {
        public override void Up()
        {
            // Add new columns with proper constraints and default values
            AddColumn("dbo.Products", "IsActive", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Products", "IsFeatured", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Products", "CreatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
            AddColumn("dbo.Products", "LastUpdated", c => c.DateTime());
            AddColumn("dbo.Products", "CreatedBy", c => c.String(maxLength: 100));
            AddColumn("dbo.Products", "LastModifiedBy", c => c.String(maxLength: 100));

            // Set default values for existing records
            Sql("UPDATE dbo.Products SET IsActive = 1, IsFeatured = 0, CreatedAt = GETDATE()");

            // Optional: Set default CreatedBy for existing records
            Sql("UPDATE dbo.Products SET CreatedBy = 'System' WHERE CreatedBy IS NULL");
        }

        public override void Down()
        {
            DropColumn("dbo.Products", "LastModifiedBy");
            DropColumn("dbo.Products", "CreatedBy");
            DropColumn("dbo.Products", "LastUpdated");
            DropColumn("dbo.Products", "CreatedAt");
            DropColumn("dbo.Products", "IsFeatured");
            DropColumn("dbo.Products", "IsActive");
        }
    }
}



//namespace Zaptech.Migrations
//{
//    using System;
//    using System.Data.Entity.Migrations;

//    public partial class AddProductFields : DbMigration
//    {
//        public override void Up()
//        {
//            AddColumn("dbo.Products", "IsActive", c => c.Boolean(nullable: false));
//            AddColumn("dbo.Products", "IsFeatured", c => c.Boolean(nullable: false));
//            AddColumn("dbo.Products", "CreatedAt", c => c.DateTime(nullable: false));
//            AddColumn("dbo.Products", "LastUpdated", c => c.DateTime());
//            AddColumn("dbo.Products", "CreatedBy", c => c.String());
//            AddColumn("dbo.Products", "LastModifiedBy", c => c.String());
//        }

//        public override void Down()
//        {
//            DropColumn("dbo.Products", "LastModifiedBy");
//            DropColumn("dbo.Products", "CreatedBy");
//            DropColumn("dbo.Products", "LastUpdated");
//            DropColumn("dbo.Products", "CreatedAt");
//            DropColumn("dbo.Products", "IsFeatured");
//            DropColumn("dbo.Products", "IsActive");
//        }
//    }
//}
