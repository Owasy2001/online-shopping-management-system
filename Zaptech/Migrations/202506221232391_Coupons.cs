namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Coupons : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Coupons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 20),
                        DiscountType = c.String(nullable: false),
                        DiscountValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        MinimumOrderAmount = c.Decimal(precision: 18, scale: 2),
                        MaxUses = c.Int(),
                        CurrentUses = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Orders", "Coupon_Id", c => c.Int());
            CreateIndex("dbo.Orders", "Coupon_Id");
            AddForeignKey("dbo.Orders", "Coupon_Id", "dbo.Coupons", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "Coupon_Id", "dbo.Coupons");
            DropIndex("dbo.Orders", new[] { "Coupon_Id" });
            DropColumn("dbo.Orders", "Coupon_Id");
            DropTable("dbo.Coupons");
        }
    }
}
