-- compatibility level min: 150
DECLARE @s VARCHAR(100) = dbo.unknown_fn()

IF  TRIM('X' FROM @s) <> 'asXdf'
OR LTRIM(@s) <> 'asdf   '
OR RTRIM(@s, 'X') =  'Xasdf'
BEGIN
    PRINT '?'
END

-- no comparison
SELECT 1
WHERE LTRIM(@s, 'X') IS NOT NULL

-- no precise value
SELECT 1
WHERE RTRIM(@s, '') IS NOT NULL

SELECT 1
WHERE TRIM(dbo.som_fn() FROM @s) IS NOT NULL
