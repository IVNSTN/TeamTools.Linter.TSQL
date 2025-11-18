-- var is initialized with precisely NULL
DECLARE @substr VARCHAR(20);

IF @substr IS NOT NULL
    AND 'asdf' = (@substr + 'df')
BEGIN
    PRINT 1;
END

IF NOT (@substr IS NULL)
    AND 'asdf' = (@substr + 'df')
BEGIN
    PRINT 2;
END
GO

-- var is initialized with UNKNOWN
DECLARE @substr VARCHAR(20) = dbo.unknown_fn();

IF @substr IS NOT NULL
    AND 'asdf' = (@substr + 'df')
BEGIN
    PRINT 1;
END

IF NOT (@substr IS NULL)
    AND 'asdf' = (@substr + 'df')
BEGIN
    PRINT 2;
END
GO
