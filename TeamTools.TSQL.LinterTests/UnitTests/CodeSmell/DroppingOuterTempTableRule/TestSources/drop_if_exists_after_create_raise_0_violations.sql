-- compatibility level min: 130
CREATE PROC foo
AS
BEGIN
    CREATE TABLE #tbl (id INT);

    DROP TABLE IF EXISTS #tbl;
END;
GO
