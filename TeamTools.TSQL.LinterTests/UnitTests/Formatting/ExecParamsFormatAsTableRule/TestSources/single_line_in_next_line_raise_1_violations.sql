declare @res int;
exec @res = dbo.foo 
@bar = 1, @zar = 2;
