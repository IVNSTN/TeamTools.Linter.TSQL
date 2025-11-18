CREATE TYPE dbo.foo AS TABLE
(
    id      INT            NOT NULL
    , value DECIMAL(18, 6) NULL
    , dt    DATETIME2(7)   NOT NULL
);
GO
