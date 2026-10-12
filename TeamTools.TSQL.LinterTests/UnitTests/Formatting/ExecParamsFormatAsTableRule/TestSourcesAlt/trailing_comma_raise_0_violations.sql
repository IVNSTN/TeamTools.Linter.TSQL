declare @res int;
execute @res = dbo.foo
    @bar = 1,
    @far = 2,
    @zar = 3;
