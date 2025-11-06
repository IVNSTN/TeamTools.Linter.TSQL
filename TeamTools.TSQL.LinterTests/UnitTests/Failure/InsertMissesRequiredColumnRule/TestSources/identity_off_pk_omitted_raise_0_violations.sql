CREATE TABLE #tbl
(
    id    INT NOT NULL IDENTITY(1,1)
    , val INT NULL
)

INSERT #tbl(val) -- id omitted
VALUES (3)
