DECLARE @s VARCHAR(100) = dbo.unknown_fn()

IF  TRIM(@s) <> '  asdf  '
OR LTRIM(@s) <> '  asdf'
OR RTRIM(@s) =  'asdf  '
BEGIN
    PRINT '?'
END
