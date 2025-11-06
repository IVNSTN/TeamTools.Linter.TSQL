IF (@a > @b) AND (ABS(@c) / a * 2 > 0 AND (@a > @b))
    PRINT 1;
GO

IF (ABS(@c) / a * 2 > 0 OR (@a > @b)) AND (@a > @b)
    PRINT 1;
GO
