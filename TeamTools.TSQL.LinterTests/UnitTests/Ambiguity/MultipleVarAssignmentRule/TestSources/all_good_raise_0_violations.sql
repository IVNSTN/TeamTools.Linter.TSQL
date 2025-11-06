DECLARE @res INT, @p INT

EXEC @res = dbo.test
    @param = @p OUTPUT

EXEC dbo.test
    @param = @res OUTPUT

EXEC @res = dbo.test
    @param = @res -- no output

EXEC dbo.test
GO
DECLARE @cr CURSOR;

FETCH NEXT FROM @cr;

FETCH NEXT FROM @cr
INTO @a, @b, @c;
