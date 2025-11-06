SELECT COUNT((SELECT TOP(1) t.id FROM dbo.too t)) OVER (PARTITION BY parent_id)
FROM dbo.foo

SELECT SUM((SELECT TOP(1) t.id FROM dbo.too t)) OVER (PARTITION BY parent_id)
FROM dbo.foo

SELECT AVG((SELECT TOP(1) t.id FROM dbo.too t)) OVER (PARTITION BY parent_id)
FROM dbo.foo
