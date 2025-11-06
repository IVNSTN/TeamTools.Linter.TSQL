-- compatibility level min: 130
DECLARE @tbl TABLE
(
    txt TEXT
    , INDEX ix CLUSTERED (txt) -- here
)
