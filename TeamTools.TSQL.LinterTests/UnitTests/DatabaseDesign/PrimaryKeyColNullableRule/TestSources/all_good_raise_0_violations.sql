CREATE TABLE dbo.acme
(
    id   INT NOT NULL
    , dt DATE NULL
    , CONSTRAINT PK_FOO PRIMARY KEY CLUSTERED (id)
    , UNIQUE (dt)
)

alter table dbo.acme add constraint PK_acme
PRIMARY KEY NONCLUSTERED (id)

DECLARE @foo TABLE
(
    id       INT  not null IDENTITY(1,1)
    , PRIMARY KEY CLUSTERED (id)
)
