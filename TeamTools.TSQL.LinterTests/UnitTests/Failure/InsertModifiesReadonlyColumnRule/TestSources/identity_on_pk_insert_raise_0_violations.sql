CREATE TABLE #tbl
(
    id INT NOT NULL IDENTITY(1,1)
    , val INT NULL
)

SET IDENTITY_INSERT #tbl ON

INSERT #tbl(id, val) -- id can be inserted
VALUES (1, 3)

SET IDENTITY_INSERT #tbl OFF
