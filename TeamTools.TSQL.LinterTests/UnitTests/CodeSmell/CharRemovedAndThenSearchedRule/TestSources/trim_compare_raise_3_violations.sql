-- compatibility level min: 150
DECLARE @s VARCHAR(100) = dbo.unknown_fn()

IF  TRIM('X' FROM @s) <> 'XXX asdf'
OR LTRIM(@s, 'X') <> 'XXX asdf'
OR RTRIM(@s, 'X') =  'asdf XXX'
BEGIN
    PRINT '?'
END
