SELECT 1
FROM dbo.foo AS f
INNER JOIN scm.bar AS b
    ON scm.bar.title LIKE f.pattern
