CREATE TABLE dbo.acme
(
    id int not null,
    a_date date not null
) on Date2fsMessBess(a_date)
GO
CREATE INDEX ix_foo
on dbo.acme(id)
on [PRIMARY]
GO
CREATE INDEX ix_bar
on dbo.acme(id)
on Date2fsMessBess(a_date)
GO
