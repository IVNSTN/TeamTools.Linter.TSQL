SELECT *
FROM foo AS f
LEFT JOIN dbo.bar AS b
ON child_id = parent_id
-- make this query a little bit complicated
INNER JOIN far
ON far.something = f.src_something
CROSS APPLY dbo.my_fn(f.x) as y
WHERE 1 = bar.value
    AND dbo.fn() IS NOT NULL

SELECT *
FROM dbo.bar
RIGHT JOIN schm.foo
ON child_id = parent_id
WHERE bar.value < schm.foo.value_limit
