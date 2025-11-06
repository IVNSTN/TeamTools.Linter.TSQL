CREATE TABLE dbo.foo
(
    row_id  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
    , dt    DATETIME
    , title VARCHAR(100)
    , CONSTRAINT PK PRIMARY KEY (row_id, dt) -- 1
)

CREATE UNIQUE NONCLUSTERED INDEX IX ON dbo.foo (dt, row_id) -- 2
GO

CREATE TABLE dbo.bar
(
    row_id  UNIQUEIDENTIFIER NOT NULL
    , dt    DATETIME
    , title VARCHAR(100)
    , CONSTRAINT PK PRIMARY KEY (row_id, dt) -- 3
)
GO

ALTER TABLE dbo.bar
    ADD CONSTRAINT DF_UUID DEFAULT NEWID() FOR row_id -- default added by alter statement
GO

CREATE UNIQUE NONCLUSTERED INDEX IX ON dbo.bar (dt, row_id) -- 4
GO
