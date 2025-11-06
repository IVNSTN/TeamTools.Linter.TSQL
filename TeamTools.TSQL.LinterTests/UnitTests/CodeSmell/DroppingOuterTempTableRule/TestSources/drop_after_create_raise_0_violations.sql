CREATE TRIGGER bar
ON zar
AFTER INSERT
AS
BEGIN
    CREATE TABLE #tbl (id INT);

    DROP TABLE #tbl;
END;
GO

CREATE TRIGGER bar
ON zar
AFTER INSERT
AS
BEGIN
    SELECT *
    INTO #tbl -- same as create
    FROM inserted;

    DROP TABLE #tbl;
END;
GO
