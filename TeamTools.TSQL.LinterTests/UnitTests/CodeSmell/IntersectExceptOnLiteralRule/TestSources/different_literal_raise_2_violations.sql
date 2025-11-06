SELECT 1 AS id, t.a, NULL AS name FROM dbo.foo
INTERSECT
SELECT 1 AS id, t.a, 't' AS name FROM dbo.bar
EXCEPT
SELECT 0, t.a, 'z' FROM dbo.zar;
