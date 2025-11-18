SELECT a
FROM dbo.foo AS f
INNER JOIN dbo.bar AS b
    ON b.id = (SELECT Id FROM c WHERE c.id = f.id)
