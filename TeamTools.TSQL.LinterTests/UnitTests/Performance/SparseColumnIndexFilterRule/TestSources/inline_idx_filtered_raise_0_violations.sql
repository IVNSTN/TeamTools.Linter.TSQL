-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    bar INT SPARSE NULL
    , not_sparse DATETIME NOT NULL
    , INDEX ix (bar) WHERE bar IS NOT NULL
    , INDEX dt (not_sparse)
)
GO
