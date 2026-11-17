-- Run once against the MyStore database before using stock-aware pages.
-- Existing products start at 0; enter their actual stock on Admin > Products > Edit.
IF COL_LENGTH(N'dbo.Product', N'Quantity') IS NULL
BEGIN
    ALTER TABLE dbo.Product
    ADD Quantity int NOT NULL CONSTRAINT DF_Product_Quantity DEFAULT (0) WITH VALUES;
END;
