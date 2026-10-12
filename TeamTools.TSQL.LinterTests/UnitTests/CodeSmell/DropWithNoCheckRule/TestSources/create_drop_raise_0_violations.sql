-- separate constraint definition
ALTER TABLE dbo.foo ADD CONSTRAINT DF_amount DEFAULT 0 FOR amount;

ALTER TABLE dbo.foo DROP CONSTRAINT DF_amount;
GO

-- inline constraint definition
ALTER TABLE dbo.bar ADD qty INT NOT NULL CONSTRAINT DF_qty DEFAULT 0;

ALTER TABLE dbo.bar DROP CONSTRAINT DF_qty;
GO

CREATE TABLE #tmp (id INT)
DROP TABLE #tmp
GO
