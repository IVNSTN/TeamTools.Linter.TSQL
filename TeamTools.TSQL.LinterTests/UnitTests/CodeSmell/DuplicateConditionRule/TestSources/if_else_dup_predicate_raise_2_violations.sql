IF @a > 1
    PRINT 1
ELSE IF @c = @d
    PRINT 2
ELSE IF @a > 1 -- 1
    PRINT 3

GO

IF @a <> @d
    PRINT 1
ELSE IF @c = @d
    PRINT 2
ELSE IF @a <> @d -- 2
    PRINT 3
GO
