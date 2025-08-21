namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUserTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "StoreName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "StoreName");
        }
    }
}
