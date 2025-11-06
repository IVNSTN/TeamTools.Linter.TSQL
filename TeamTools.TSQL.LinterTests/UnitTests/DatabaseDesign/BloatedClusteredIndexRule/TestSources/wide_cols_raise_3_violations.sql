CREATE TABLE one
(
    NumEDocument BIGINT        NOT NULL
    , FiCode     VARCHAR(MAX)  NOT NULL
    , Comment    VARCHAR(1024) NOT NULL
    , PRIMARY KEY CLUSTERED (NumEDocument, FiCode) -- MAX varchar
);
GO

CREATE TABLE two
(
    NumEDocument BIGINT        NOT NULL
    , FiCode     VARCHAR(512)  NOT NULL
    , Comment    VARCHAR(1024) NOT NULL
    , PRIMARY KEY CLUSTERED (NumEDocument, FiCode, Comment) -- big varchars
);
GO

CREATE TABLE three
(
    NumEDocument BIGINT           NOT NULL
    , foo        DECIMAL(38, 8)   NOT NULL
    , bar        NCHAR(120)       NOT NULL
    , far        FLOAT(24)        NOT NULL
    , jar        UNIQUEIDENTIFIER NOT NULL
    , car        DATETIME2(7)     NOT NULL
    , PRIMARY KEY NONCLUSTERED (NumEDocument)
);
GO

CREATE CLUSTERED INDEX IX1
    ON three (foo, bar, far, jar, car); -- total size is big
GO

CREATE NONCLUSTERED INDEX IX2
    ON three (NumEDocument);
GO
