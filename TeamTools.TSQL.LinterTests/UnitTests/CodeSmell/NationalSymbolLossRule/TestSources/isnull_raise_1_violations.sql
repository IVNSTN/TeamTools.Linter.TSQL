DECLARE @s CHAR(10)

-- Second argument of ISNULL is always cast to the first argument type
-- here unicode symbol will be lost during conversion to 1-byte char string
PRINT ISNULL(@s, N'☀️')
