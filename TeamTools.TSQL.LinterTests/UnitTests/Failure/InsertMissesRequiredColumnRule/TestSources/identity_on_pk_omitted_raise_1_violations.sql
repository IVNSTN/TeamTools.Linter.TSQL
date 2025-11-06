CREATE TABLE #tbl
(
    id INT NOT NULL IDENTITY(1,1)
    , val INT NULL
)

SET IDENTITY_INSERT #tbl ON

INSERT #tbl(val) -- id must be inserted explicitly
VALUES (3)

SET IDENTITY_INSERT #tbl OFF
