CREATE TABLE dbo.acme
(
    id int not null,
    rn timestamp not null
)
GO
CREATE UNIQUE INDEX ix_foo
on dbo.acme(rn)
GO
CREATE index ix_bar
on dbo.acme(id)
