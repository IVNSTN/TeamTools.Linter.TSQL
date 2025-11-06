CREATE TABLE dbo.client_balance
(
    a_date          DATETIME       NOT NULL
    , p_code        VARCHAR(12)    NOT NULL
    , account_code  VARCHAR(21)    NOT NULL
    , income_rest   NUMERIC(18, 2) NOT NULL
    , real_rest     NUMERIC(18, 2) NOT NULL
    , long_forward  NUMERIC(18, 2) NOT NULL
    , short_forward NUMERIC(18, 2) NOT NULL
    , long_plan     NUMERIC(14, 2) NOT NULL
    , ver           ROWVERSION     NOT NULL
    , CONSTRAINT PK_client_balance PRIMARY KEY CLUSTERED (place_code ASC, account_code ASC, a_date ASC) WITH (DATA_COMPRESSION = PAGE) ON prtschm([a_date]));
GO

CREATE UNIQUE NONCLUSTERED INDEX uq
    ON dbo.client_balance (ver ASC);
GO
