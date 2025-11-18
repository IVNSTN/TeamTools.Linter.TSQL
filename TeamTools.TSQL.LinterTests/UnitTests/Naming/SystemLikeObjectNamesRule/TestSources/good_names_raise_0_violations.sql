create table test (a int);
GO
create view foo as select * from bar
GO
create trigger trg on dbo.test after update as BEGIN ROLLBACK END;
GO
create function zar (@id int) returns int AS begin return 1 end
GO
create procedure dbo.jar as ;
GO
create type custom_int from int;
GO
create synonym here_far for there_jar;
