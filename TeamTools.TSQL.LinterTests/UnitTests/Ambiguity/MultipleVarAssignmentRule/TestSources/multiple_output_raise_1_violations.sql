DECLARE @res INT

EXEC dbo.test
    @a = @res OUTPUT
    , @b = @res OUTPUT
