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

UPDATE t SET
    a = b
    OUTPUT INSERTED.a
    INTO #tbl(val)
FROM t

MERGE #tbl
USING s
ON t.id = s.id
WHEN NOT MATCHED THEN
    INSERT (val)
    VALUES (s.a);
GO

-- these dont do INSERT

UPDATE t SET
    a = b
    OUTPUT INSERTED.a
FROM t

MERGE t
USING s
ON t.id = s.id
WHEN MATCHED THEN
    DELETE;
GO
