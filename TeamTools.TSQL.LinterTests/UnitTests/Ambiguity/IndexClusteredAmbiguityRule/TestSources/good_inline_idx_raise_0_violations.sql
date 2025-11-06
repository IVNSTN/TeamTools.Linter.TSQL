-- compatibility level min: 130
CREATE TABLE dbo.bar
(
    id           INT NOT NULL
    , order_date DATETIME
    , INDEX IX_order_date NONCLUSTERED (order_date)
    , UNIQUE NONCLUSTERED (order_date)
);
