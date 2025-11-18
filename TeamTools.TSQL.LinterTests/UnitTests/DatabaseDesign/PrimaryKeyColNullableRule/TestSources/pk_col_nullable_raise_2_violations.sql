CREATE TABLE dbo.acme
(
    id   INT NULL
    , CONSTRAINT PK_FOO PRIMARY KEY CLUSTERED (id) -- 1
)
GO

CREATE TABLE dbo.foobar
(
    id   INT NULL
)

alter table dbo.foobar add constraint PK_foobar
PRIMARY KEY NONCLUSTERED (id) -- 2
