CREATE PROC foo
AS
BEGIN
    DECLARE @tbl TABLE (id INT);

    DROP TABLE dbo.foo; -- not a #

    EXEC ('drop table #tbl'); -- dynamic sql ignored
END;
GO

CREATE TRIGGER bar
ON zar
AFTER INSERT
AS
BEGIN
    DECLARE @tbl TABLE (id INT);

    DROP TABLE dbo.foo; -- not a #

    EXEC ('drop table #tbl'); -- dynamic sql ignored
END;
