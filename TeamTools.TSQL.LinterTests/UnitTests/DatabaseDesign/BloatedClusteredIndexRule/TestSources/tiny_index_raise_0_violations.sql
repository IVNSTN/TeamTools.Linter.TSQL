CREATE TABLE dbo.edocuments
(
    DocumentNumber                  BIGINT         NOT NULL
    , IdDocumentType                TINYINT        NOT NULL
    , IdCommandType                 INT            NOT NULL
    , SignTime                      DATETIME       NOT NULL
    , PRIMARY KEY CLUSTERED (DocumentNumber)
);
GO

CREATE TABLE foo.bar
(
    id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
);
GO
