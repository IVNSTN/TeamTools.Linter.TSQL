CREATE TABLE #tbl
(
    id    INT NOT NULL IDENTITY(1,1)
    , val INT NULL
    , rv  ROWVERSION NOT NULL
    , tm  TIMESTAMP  NOT NULL
)

INSERT #tbl(val) -- rv, tm, id will be populated implicitly
VALUES (3)
