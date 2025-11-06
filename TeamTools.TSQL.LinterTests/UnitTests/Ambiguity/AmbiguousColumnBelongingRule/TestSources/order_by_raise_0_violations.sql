SELECT 'asfd' AS title
FROM dbo.foo AS f
INNER JOIN dbo.bar AS b
    ON f.id = b.id
ORDER BY title;
