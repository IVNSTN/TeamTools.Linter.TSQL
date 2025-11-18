declare @res int;
execute @res = dbo.foo
    @bar = 1,
    @zar = 2;
