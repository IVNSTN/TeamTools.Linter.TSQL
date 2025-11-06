declare @res int;
exec @res = dbo.foo 
'asdf', @bar = 1, @zar = 2;
