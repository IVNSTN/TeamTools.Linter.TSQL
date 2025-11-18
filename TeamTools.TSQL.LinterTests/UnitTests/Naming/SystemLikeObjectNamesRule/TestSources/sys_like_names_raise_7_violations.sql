create table sys_test (a int);
GO
create view xp_foo as select * from bar
GO
create trigger sysmail_trg on dbo.test after update as BEGIN ROLLBACK END;
GO
create function sys_zar (@id int) returns int AS begin return 1 end
GO
create procedure dbo.sys_jar as ;
GO
create type xp_int from int;
GO
create synonym sys_far for sys_jar;
