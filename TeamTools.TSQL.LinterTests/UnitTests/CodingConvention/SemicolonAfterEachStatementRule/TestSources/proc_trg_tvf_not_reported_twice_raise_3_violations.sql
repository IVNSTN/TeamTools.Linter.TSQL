CREATE PROC dbo.my_proc
AS
SELECT TOP 100 PERCENT sc.name, sc.id, sc.address, sc.region, sc.map, r.name AS region_name, e.sys_name
FROM  dbo.schools
ORDER BY sc.id
GO

CREATE TRIGGER dbo.my_trg ON dbo.my_tbl AFTER DELETE
AS
RAISERROR('asdf', 16, 1)
GO

CREATE FUNCTION dbo.my_fn(@num NUMERIC(8,2))
RETURNS @out TABLE (id int)
AS
BEGIN
    -- SELECT 1 as id
    RETURN;
END
