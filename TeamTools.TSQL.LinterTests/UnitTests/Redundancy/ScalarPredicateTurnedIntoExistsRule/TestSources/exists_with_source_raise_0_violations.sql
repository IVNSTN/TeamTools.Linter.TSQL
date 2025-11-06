IF EXISTS(SELECT 1 FROM dbo.far WHERE @a > @b)
BEGIN
    SELECT @c = CASE WHEN EXISTS(SELECT a, b, c FROM dbo.jar WHERE dbo.fn() IS NOT NULL) THEN 1
        ELSE 2
        END
    FROM dbo.bar b
    WHERE @a > b.a
        AND NOT EXISTS(SELECT 0/0 FROM dbo.zar WHERE DATEADD(DAY, 1, b.dt) < @limit)
END ELSE BEGIN
    WHILE @a > 1 AND EXISTS(SELECT 1 FROM dbo.car WHERE @b < @c)
        PRINT 'ASDF'
END
