-- no float
DECLARE @i INT, @n DECIMAL(10, 2)

SET @i = @n

SELECT @i + @n
WHERE @n < @n
GO

-- same type
DECLARE @f FLOAT, @r REAL

SET @f = @r

SELECT @f + @r
WHERE @f < @r
GO

-- explicit conversions
DECLARE @i INT, @f FLOAT

SELECT @i
    + CONVERT(INT, @f)
    + CAST('0.0' AS INT)

SELECT CAST(@i AS FLOAT)
    + @f
    + CAST('0.0' AS REAL)
GO
