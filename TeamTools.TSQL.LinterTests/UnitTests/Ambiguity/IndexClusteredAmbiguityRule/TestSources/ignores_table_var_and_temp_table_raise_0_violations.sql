-- compatibility level min: 130
CREATE TABLE #bar
(
    id INT NOT NULL
    , order_date DATETIME
    , INDEX IX_order_date (order_date)
    , UNIQUE (order_date)
)

DECLARE @bar TABLE
(
    id INT NOT NULL
    , order_date DATETIME
    , INDEX IX_order_date (order_date)
    , UNIQUE (order_date)
)
