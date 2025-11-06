IF @foo = @bar
    SELECT @far;

IF @foo != @bar
    PRINT 1
ELSE
    PRINT 2
GO

IF @foo != @bar
    PRINT 1
ELSE IF @far > @jar
    PRINT 2
