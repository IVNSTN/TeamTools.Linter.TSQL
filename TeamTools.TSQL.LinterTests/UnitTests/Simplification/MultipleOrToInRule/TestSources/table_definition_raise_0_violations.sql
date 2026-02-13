CREATE TABLE dbo.foo
(
    bar CHAR(10)
    , computed_col AS (CASE WHEN bar = 'B' OR bar = 'A' OR bar = 'R' THEN 1 ELSE 0 END)
    , CONSTRAINT CK_BAR_FILTER CHECK (bar = 'B' OR bar = 'A' OR bar = 'R')
)
