SELECT 'asfd' AS title
FROM dbo.foo AS f
INNER JOIN dbo.bar AS b
    ON f.id = b.id
ORDER BY title;

SELECT 'asfd' AS title
FROM (
    SELECT TOP 100 PERCENT a, b, c
    FROM foo as f
    ORDER BY x, y, z
) AS f
INNER JOIN dbo.bar AS b
    ON f.id = b.id
ORDER BY title;
