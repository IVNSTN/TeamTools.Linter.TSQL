SELECT *
FROM dbo.foo f
INNER JOIN dbo.bar b
ON ISNULL(f.id, 0) <> b.id
WHERE NULLIF(t.title, '') <> ''
