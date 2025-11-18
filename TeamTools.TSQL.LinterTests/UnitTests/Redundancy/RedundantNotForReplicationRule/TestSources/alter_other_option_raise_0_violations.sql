-- compatibility level min: 130
ALTER TABLE dbo.foob
ALTER COLUMN sys_end_time
ADD HIDDEN;
GO

ALTER TABLE dbo.bar
ALTER COLUMN bits
DROP SPARSE;
GO
