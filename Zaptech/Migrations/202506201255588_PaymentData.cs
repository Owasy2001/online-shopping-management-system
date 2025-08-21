namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class PaymentData : DbMigration
    {
        public override void Up()
        {
            Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Payments WHERE OrderId = 1)
BEGIN
    INSERT INTO dbo.Payments (OrderId, Status, Transiction_Id, Paid_at)
    VALUES
    (1, 'Paid', 'TXN2001', GETDATE()),
    (2, 'Paid', 'TXN2002', GETDATE()),
    (3, 'Unpaid', 'TXN2003', GETDATE()),
    (4, 'Paid', 'TXN2004', GETDATE()),
    (5, 'Paid', 'TXN2005', GETDATE()),
    (6, 'Pending', 'TXN2006', GETDATE()),
    (7, 'Paid', 'TXN2007', GETDATE()),
    (8, 'Paid', 'TXN2008', GETDATE()),
    (9, 'Unpaid', 'TXN2009', GETDATE()),
    (10, 'Paid', 'TXN2010', GETDATE());
END
");
        }


        public override void Down()
        {
            Sql("DELETE FROM dbo.Payments WHERE Transiction_Id IN ('TXN2001','TXN2002','TXN2003','TXN2004','TXN2005','TXN2006','TXN2007','TXN2008','TXN2009','TXN2010')");
        }
    }
}
