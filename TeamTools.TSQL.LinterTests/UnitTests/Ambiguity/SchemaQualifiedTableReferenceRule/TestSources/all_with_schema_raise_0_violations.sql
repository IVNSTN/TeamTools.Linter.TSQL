CREATE INDEX ix ON dbo.test (a)
GO
ALTER TABLE dbo.foo add x int;
GO
DROP TABLE dbo.jar;
GO
CREATE STATISTICS stats on dbo.bar (x)
GO
CREATE TRIGGER io ON dbo.zar after insert as rollback;
