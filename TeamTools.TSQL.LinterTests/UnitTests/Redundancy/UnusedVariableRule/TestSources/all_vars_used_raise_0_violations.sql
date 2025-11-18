declare @foo bit, @bar xml;

exec dbo.acme
    @arg = @FOO out;

-- undeclared variable ignored
set @b = 0;

select @bar as [bar];

declare @test table (id int null)

-- case insensitive rule
update @teST set id = 1 where 1=0;

declare @y int = 0, @rc int;
exec @rc = sp_executesql N'select @x', N'@x int', @y;
