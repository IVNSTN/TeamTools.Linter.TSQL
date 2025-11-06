/* exec test */
select * from test;

exec dbo.test

declare @var varchar(10)
exec @var -- ignored
