SET @a =
    CASE @b
        WHEN '1' THEN 1
        WHEN @c THEN 2
        WHEN (@x * 2 - ABS(@z/100)) THEN 4
    END
GO

SET @a =
    CASE
        WHEN @b = '1' THEN 1
        WHEN NOT(@b = '1') THEN -1
        WHEN @c = 700 THEN 2
        WHEN (DATEADD(DD, 1, @dt) > GETDATE() AND @x * 2 < ABS(@z/100)) THEN 4
    END

SELECT
    CASE
        WHEN BuyDate > SellDate THEN
            1
        WHEN SellDate > BuyDate THEN
            2
    END
GO

SELECT
    COALESCE(t.id, @var, '1')
    , COALESCE(t.id, @var, @c, @e)
    , COALESCE(t.id, DATEADD(DD, 1, GETDATE() + 2), @d)
GO

IF @a > 1
    PRINT 1
ELSE IF @c = @d
    PRINT 2
ELSE IF @a < 0
    PRINT 3
GO

IF @request_id IS NULL
    OR @operation_id IS NULL
BEGIN;
    PRINT '1'
END;
GO
