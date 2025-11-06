CREATE TABLE dbo.acme
(
    id int not null,
    RN rowversion not null
)
GO
CREATE UNIQUE INDEX ix_foo
on dbo.acme(rn)
GO
CREATE index ix_bar
on dbo.acme(id)
go
CREATE UNIQUE INDEX ix_zar
on dbo.acme(rn, id)
GO
