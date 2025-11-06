-- compatibility level min: 130
CREATE TABLE dbo.bar
(
    col BIT SPARSE NULL
    , INDEX ix CLUSTERED (col) -- here
)
