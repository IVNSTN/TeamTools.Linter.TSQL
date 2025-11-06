CREATE TABLE dbo.client_balance
(
    a_date          DATETIME       NOT NULL
    , p_code        VARCHAR(12)    NOT NULL
    , place_code    VARCHAR(20)    NOT NULL
    , acc_code      VARCHAR(21)    NOT NULL
    , income_rest   NUMERIC(18, 2) NOT NULL
    , real_rest     NUMERIC(18, 2) NOT NULL
    , long_forword  NUMERIC(18, 2) NOT NULL
    , short_forword NUMERIC(18, 2) NOT NULL
    , long_plan     NUMERIC(14, 2) NOT NULL
    , ver           ROWVERSION     NOT NULL
    , CONSTRAINT PK_client_balance PRIMARY KEY CLUSTERED (place_code ASC, acc_code ASC, paper_no ASC, a_date ASC) WITH (DATA_COMPRESSION = PAGE)
) ON [PRIMARY];
GO

CREATE UNIQUE NONCLUSTERED INDEX uq_client_balance_ver
    ON dbo.client_balance (ver ASC);
GO
