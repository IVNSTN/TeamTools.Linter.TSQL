-- compatibility level min: 130
CREATE PROC foo
AS
BEGIN
    DROP TABLE IF EXISTS #tbl; -- 1

    CREATE TABLE #tbl (id INT);
END;
GO
