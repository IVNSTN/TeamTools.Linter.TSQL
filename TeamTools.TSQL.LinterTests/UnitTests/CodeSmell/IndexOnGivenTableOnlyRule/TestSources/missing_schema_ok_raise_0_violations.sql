CREATE TABLE foo
(
    id int not null);
GO
CREATE INDEX ix_dbo_foo
on foo(id)
on [PRIMARY]
GO
CREATE INDEX ix_foo
on foo(id)
