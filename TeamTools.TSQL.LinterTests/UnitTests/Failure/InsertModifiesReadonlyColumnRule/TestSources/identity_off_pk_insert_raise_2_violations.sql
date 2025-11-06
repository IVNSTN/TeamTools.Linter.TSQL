CREATE TABLE #tbl
(
    id INT NOT NULL IDENTITY(1,1)
    , val INT NULL
)

SET IDENTITY_INSERT #tbl OFF

INSERT #tbl(id, val) -- id cannot be inserted
VALUES (1, 3)

INSERT #tbl(id, val) -- id cannot be inserted
EXEC dbo.bar

SET IDENTITY_INSERT #tbl ON
