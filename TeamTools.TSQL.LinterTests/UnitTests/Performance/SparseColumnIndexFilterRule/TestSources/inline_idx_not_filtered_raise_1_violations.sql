-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    bar INT SPARSE NULL
    , INDEX ix (bar)
)
GO
