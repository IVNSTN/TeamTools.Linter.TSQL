SELECT *
FROM dbo.foo f
INNER JOIN bdo.bar b
ON b.id = f.id+1
WHERE (f.num - b.num) = 0

DELETE b
FROM dbo.foo f
INNER JOIN bdo.bar b
ON b.id = CASE WHEN f.parent_id IS NULL THEN b.id ELSE f.parent_id END
