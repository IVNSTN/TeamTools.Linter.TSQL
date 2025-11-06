CREATE TABLE dbo.shop_trades
(
    client_id   INT NOT NULL
    , trd_no INT NOT NULL
    , a_date DATETIME NOT NULL
    , CONSTRAINT PK_shop_trades PRIMARY KEY CLUSTERED (trd_no ASC, a_date ASC) ON prt(a_date));
GO

CREATE UNIQUE NONCLUSTERED INDEX idx
    ON dbo.shop_trades (client_id ASC, a_date)
    WITH (FILLFACTOR = 100, DATA_COMPRESSION = PAGE)
    ON prt(a_date); -- partitioned on clustered col
