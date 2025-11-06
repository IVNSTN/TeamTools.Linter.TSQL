CREATE PROC dbo.foo
AS
BEGIN
    CREATE TABLE #tmp (id INT)

    CREATE INDEX #ix ON #tmp(id)
END
