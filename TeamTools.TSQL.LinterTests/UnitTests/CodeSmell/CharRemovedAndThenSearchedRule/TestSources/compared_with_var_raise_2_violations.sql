DECLARE @s VARCHAR(100) = dbo.unknown_fn()
, @rightSide VARCHAR(100) = 'as X df'

IF REPLACE(@s, 'X', 'Y') <> @rightSide
OR REPLACE(@s, 'X', 'Y') =  @rightSide
BEGIN
    PRINT '?'
END
