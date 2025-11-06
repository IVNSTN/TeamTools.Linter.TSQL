CREATE TABLE #tbl
(
    id         INT NOT NULL IDENTITY(1, 1)
    , val      INT NULL
    , computed DECIMAL(12, 2)
);

INSERT #tbl (val, computed)
VALUES (2, 3);

INSERT #tbl (val, computed)
SELECT
    src.foo
    , src.bar
FROM dbo.zar

INSERT #tbl (val, computed)
EXEC dbo.far
GO
