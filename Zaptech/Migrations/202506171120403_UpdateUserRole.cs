namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UpdateUserRole : DbMigration
    {
        public override void Up()
        {
            // Step 1: Convert existing 'nvarchar' Role values to corresponding int values
            Sql(@"
                UPDATE dbo.Users SET Role = 1 WHERE Role = 'Customer';
                UPDATE dbo.Users SET Role = 2 WHERE Role = 'Seller';
                UPDATE dbo.Users SET Role = 99 WHERE Role = 'Admin';
            ");

            // Step 2: Now alter the column to be int
            AlterColumn("dbo.Users", "Role", c => c.Int(nullable: false, defaultValue: 1));

            // Step 3: Insert two admin users
            Sql(@"
                INSERT INTO dbo.Users (Name, Email, Password, Role, Address, Phone)
                VALUES 
                ('SuperAdmin1', 'admin1@email.com', 'adminpass1', 99, 'Admin Address 1', '01800000001'),
                ('SuperAdmin2', 'admin2@email.com', 'adminpass2', 99, 'Admin Address 2', '01800000002');
            ");
        }

        public override void Down()
        {
            // Reverse: Change Role column back to string
            AlterColumn("dbo.Users", "Role", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
