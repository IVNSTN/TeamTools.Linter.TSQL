CREATE TABLE #tbl
(
    id INT NOT NULL IDENTITY(1,1)
    , val INT NULL
    , computed AS val * 2 -- computed col
)

INSERT #tbl(val, computed)
VALUES (2, 3)

INSERT #tbl(val, computed)
VALUES (2, 3)
