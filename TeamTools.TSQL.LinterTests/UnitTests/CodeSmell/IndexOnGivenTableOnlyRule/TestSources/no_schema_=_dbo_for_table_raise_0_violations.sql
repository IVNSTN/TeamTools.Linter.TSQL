CREATE TABLE foo
(
    id int not null);
GO
CREATE INDEX ix_foo
on dbo.foo(id)
