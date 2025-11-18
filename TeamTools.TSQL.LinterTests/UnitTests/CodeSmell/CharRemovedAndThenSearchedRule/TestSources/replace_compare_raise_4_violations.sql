DECLARE @s VARCHAR(100) = dbo.unknown_fn()

IF REPLACE(@s, 'X', 'Y') <> 'XXX asdf'
OR REPLACE(@s, 'X', 'Y') = 'as X df'
BEGIN
    PRINT '?'
END

-- same comparison reversed
IF 'XXX asdf' <> REPLACE(@s, 'X', 'Y')
OR 'as X df'  =  REPLACE(@s, 'X', 'Y')
BEGIN
    PRINT '?'
END
