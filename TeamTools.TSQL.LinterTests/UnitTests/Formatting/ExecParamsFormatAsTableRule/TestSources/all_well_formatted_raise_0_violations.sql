declare @res int;
exec @res = dbo.foo_foo_foo
    @bar     = 'asdfsaf'
    , @zar   = 123.01
    , @xxx   = 'asdfasdf';

exec test;

exec dbo.foo_foo_foo 'bar', 'bar', 123; -- single-line shorthand

exec sp_executesql N'select @x', N'@x int', 1;
