-- compatibility level min: 130
CREATE TABLE #bar (id int,
INDEX ix (id),
UNIQUE CLUSTERED (id)
)
