SELECT
    (CASE WHEN @a > @b THEN 1 WHEN @c = @d THEN 2 ELSE 3 END) adsf
FROM dbo.foo
