DECLARE @res INT

EXEC @res = dbo.test
    @param = @res OUTPUT
