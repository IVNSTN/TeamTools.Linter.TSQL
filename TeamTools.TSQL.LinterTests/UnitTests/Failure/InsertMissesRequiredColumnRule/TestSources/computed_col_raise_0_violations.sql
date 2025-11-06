CREATE TABLE #tbl
(
    id INT NOT NULL
    , val INT NULL
    , computed AS val * 2 -- computed col
)

INSERT #tbl(id, val)
VALUES (2, 3)
