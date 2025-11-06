SELECT *
FROM dbo.foo f
INNER JOIN dbo.bar b
    ON f.id = b.id

SELECT *
FROM dbo.foo f
LEFT JOIN (SELECT id FROM dbo.far) a
INNER JOIN dbo.bar b
    ON a.id = b.parent_id
    ON f.id = b.id

SELECT *
FROM dbo.foo f
OUTER APPLY (SELECT TOP(1) id FROM dbo.far) a
