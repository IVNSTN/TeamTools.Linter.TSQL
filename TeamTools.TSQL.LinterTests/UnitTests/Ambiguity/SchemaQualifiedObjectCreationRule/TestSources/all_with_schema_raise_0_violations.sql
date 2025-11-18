CREATE TABLE dbo.test(a int)
go
CREATE PROCEDURE tmp.foo @a int as ;
GO
CREATE TRIGGER dbo.test_iu on dbo.test after update as rollback;
GO
CREATE view frontend.pos as select 1 as x where 1=0
GO
CREATE FUNCTION xxx.zar (@val int) returns int begin return @val-1 end;
go
CREATE type custom.string FROM varchar(10);
go
CREATE queue dbo.messages;
go
CREATE SYNONYM my.bar FOR their.foo
GO

select a, b, c
  into far.bar
  from dbo.foo
GO
