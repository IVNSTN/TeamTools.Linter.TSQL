-- wrong comma position must be detected by a separate rule
declare @res int;
execute @res = dbo.foo
    @bar = 1,
    @far = 2,
    @zar = 3;
