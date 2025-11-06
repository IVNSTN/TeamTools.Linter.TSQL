CREATE TABLE dbo.foo
(
    id int not null);
GO
CREATE INDEX ix_bar
on dbo.bar(id)
on [PRIMARY]
GO
CREATE INDEX ix_bak_foo
on bak.foo(id)
on [PRIMARY]
