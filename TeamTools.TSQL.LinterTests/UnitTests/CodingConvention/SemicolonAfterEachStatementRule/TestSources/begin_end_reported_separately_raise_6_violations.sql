CREATE PROC dbo.my_proc
AS
BEGIN
    SELECT TOP 100 PERCENT sc.name, sc.id, sc.address, sc.region, sc.map, r.name AS region_name, e.sys_name
    FROM  dbo.schools
    ORDER BY sc.id
        END
        GO

CREATE TRIGGER dbo.my_trg ON dbo.my_tbl AFTER DELETE
AS
BEGIN
    RAISERROR('asdf', 16, 1)
END
GO

CREATE FUNCTION dbo.my_fn(@num int)
RETURNS @out TABLE (id int)
AS
BEGIN
    SELECT 1 as id
    ORDER BY 2
END
