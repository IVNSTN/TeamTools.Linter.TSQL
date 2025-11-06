CREATE TABLE test(a int)
go
CREATE PROCEDURE foo @a int as ;
GO
CREATE TRIGGER test_iu on dbo.test after update AS ROLLBACK;
GO
CREATE view pos as select 1 as x where 1=0
GO
CREATE FUNCTION zar (@val int) returns int begin return @val-1 end;
go
CREATE type string FROM varchar(10);
go
CREATE queue messages;
go
