DECLARE @s VARCHAR(100) = dbo.unknown_fn()

IF REPLACE(@s, 'X', 'Y') <> 'ZZZ asdf'
OR REPLACE(@s, 'X', 'Y') = 'as Z df'
BEGIN
    PRINT '?'
END

-- same comparison reversed
IF 'ZZZ asdf' <> REPLACE(@s, 'X', 'Y')
OR 'as Z df'   = REPLACE(@s, 'X', 'Y')
BEGIN
    PRINT '?'
END

IF  TRIM(@s) <> 'as X df'
OR LTRIM(@s) <> 'asdf   '
OR RTRIM(@s) =  '   asdf'
BEGIN
    PRINT '?'
END


-- no comparison
SELECT REPLACE(@s, 'X', 'Y')
WHERE TRIM(@s) IS NOT NULL

-- no precise value

SELECT REPLACE(@s, @c, 'Y')
WHERE TRIM(@s) IS NOT NULL

SELECT REPLACE(@s, NULL, 'Y')
WHERE RTRIM(@s) IS NOT NULL


-- var value is unknown
DECLARE @rightSide VARCHAR(100) = dbo.unknown_fn(123)

IF REPLACE(@s, 'X', 'Y') = @rightSide
BEGIN
    PRINT '?'
END
