SELECT 1 AS id, t.a, NULL AS name FROM dbo.foo
INTERSECT
SELECT 1 AS id, t.a, NULL AS name FROM dbo.bar
EXCEPT
SELECT 1 AS id, t.a, zar.title FROM dbo.zar;
