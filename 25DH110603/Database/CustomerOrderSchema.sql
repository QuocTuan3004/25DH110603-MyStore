-- Chay mot lan tren database MyStore truoc khi chay ung dung.
IF COL_LENGTH(N'dbo.[Order]', N'ShippingAddress') IS NULL
   AND COL_LENGTH(N'dbo.[Order]', N'AddressDelivery') IS NOT NULL
    EXEC sys.sp_rename N'dbo.[Order].AddressDelivery', N'ShippingAddress', N'COLUMN';

IF COL_LENGTH(N'dbo.[Order]', N'DeliveryMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD DeliveryMethod nvarchar(50) NOT NULL
        CONSTRAINT DF_Order_DeliveryMethod DEFAULT (N'Tiêu chuẩn');

IF COL_LENGTH(N'dbo.[Order]', N'PaymentMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD PaymentMethod nvarchar(50) NOT NULL
        CONSTRAINT DF_Order_PaymentMethod DEFAULT (N'Thanh toán khi nhận hàng');

IF COL_LENGTH(N'dbo.OrderDetail', N'TotalPrice') IS NULL
    ALTER TABLE dbo.OrderDetail ADD TotalPrice AS (Quantity * UnitPrice) PERSISTED;
GO