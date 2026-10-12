CREATE TABLE #tbl
(
    id INT NOT NULL IDENTITY(1,1)
    , val INT NULL
    , computed AS val * 2 -- computed col
)

UPDATE t SET
    a = b
    OUTPUT INSERTED.a
    INTO #tbl(computed)     -- 1
FROM t

MERGE #tbl
USING s
ON t.id = s.id
WHEN NOT MATCHED THEN
    INSERT (computed)       -- 2
    VALUES (s.a);
GO
