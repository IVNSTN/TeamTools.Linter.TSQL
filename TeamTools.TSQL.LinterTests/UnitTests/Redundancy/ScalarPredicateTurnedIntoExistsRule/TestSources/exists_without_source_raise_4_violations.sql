IF EXISTS(SELECT 1 WHERE @a > @b) -- 1
BEGIN
    SELECT @c = CASE WHEN EXISTS(SELECT a, b, c WHERE dbo.fn() IS NOT NULL) THEN 1 -- 2
        ELSE 2
        END
    FROM dbo.bar b
    WHERE @a > b.a
        AND NOT EXISTS(SELECT 0/0 WHERE DATEADD(DAY, 1, b.dt) < @limit) -- 3
END ELSE BEGIN
    WHILE @a > 1 AND EXISTS(SELECT 1 WHERE @b < @c) -- 4
        PRINT 'ASDF'
END
