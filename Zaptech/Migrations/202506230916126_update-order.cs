namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateorder : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Orders", name: "Coupon_Id", newName: "CouponId");
            RenameIndex(table: "dbo.Orders", name: "IX_Coupon_Id", newName: "IX_CouponId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Orders", name: "IX_CouponId", newName: "IX_Coupon_Id");
            RenameColumn(table: "dbo.Orders", name: "CouponId", newName: "Coupon_Id");
        }
    }
}
