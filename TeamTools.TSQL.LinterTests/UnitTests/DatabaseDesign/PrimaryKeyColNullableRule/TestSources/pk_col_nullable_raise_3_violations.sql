CREATE TABLE dbo.acme
(
    id   INT NULL
    , CONSTRAINT PK_FOO PRIMARY KEY CLUSTERED (id) -- 1
)

alter table dbo.acme add constraint PK_acme
PRIMARY KEY NONCLUSTERED (id) -- 2

DECLARE @foo TABLE
(
    id       INT IDENTITY(1,1)
    , PRIMARY KEY CLUSTERED (id) -- 3
)
