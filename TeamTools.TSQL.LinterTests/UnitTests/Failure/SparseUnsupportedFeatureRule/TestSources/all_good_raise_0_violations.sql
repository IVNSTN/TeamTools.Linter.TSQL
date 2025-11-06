-- no sparse
CREATE TABLE dbo.foo
(
    bar    TINYINT          NULL DEFAULT 0
    , id   INT              NOT NULL IDENTITY(1, 1)
    , far  CHAR(1)          NOT NULL
    , u_id UNIQUEIDENTIFIER NULL ROWGUIDCOL
    , calc AS id + 1
);
GO

-- all valid sparse cols
CREATE TABLE dbo.bar
(
    bar    TINYINT          SPARSE NULL
    , id   INT              SPARSE NULL
    , far  CHAR(1)          SPARSE NULL
    , u_id UNIQUEIDENTIFIER SPARSE NULL
);
