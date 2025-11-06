-- compatibility level min: 130
CREATE TABLE dbo.bar
(
    id INT NOT NULL
    , order_date DATETIME
    , INDEX IX_order_date (order_date)
    , UNIQUE (order_date)
)
