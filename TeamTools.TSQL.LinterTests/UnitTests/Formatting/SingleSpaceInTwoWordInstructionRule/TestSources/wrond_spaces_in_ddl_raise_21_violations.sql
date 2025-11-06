CREATE      TABLE foo (id int)
ALTER     TABLE foo ALTER COLUMN id int
DROP    TABLE foo
GO
CREATE   VIEW foo AS select id from foo
GO
ALTER   VIEW foo AS select id from foo
GO
DROP   view foo
GO
CREATE   PROC foo AS select id from foo
GO
ALTER   PROC foo AS select id from foo
GO
DROP   PROC foo
GO
CREATE   TRIGGER trg on foo AFTER INSERT
AS RETURN;
GO
ALTER   TRIGGER trg on foo AFTER INSERT
AS RETURN;
GO
DROP   TRIGGER dbo.trg;
GO
CREATE   FUNCTION fn_bar (@id int) RETURNS TABLE
AS RETURN SELECT 1 as id
GO
ALTER   FUNCTION fn_bar (@id int) RETURNS TABLE
AS RETURN SELECT 1 as id
GO
DROP   FUNCTION fn_bar
GO
CREATE   INDEX idx on dbo.foo (parent_id)
GO
DROP   INDEX dbo.foo.idx;
GO
create   synonym sym for master.dbo.test
go
drop   synonym sym
go
create   type dbo.tp from int null
go
drop   type dbo.tp
