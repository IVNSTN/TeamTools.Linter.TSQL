SELECT *
FROM foo f
LEFT OUTER JOIN bar b
ON child_id = parent_id
WHERE b.title LIKE 'asdf%'      -- 1

SELECT *
FROM foo f
LEFT OUTER JOIN bar b
ON child_id = parent_id
WHERE b.value IN (1, 2, 3)      -- 2

SELECT *
FROM foo f
INNER JOIN jar j
ON j.id = f.id
CROSS APPLY dbo.fn(j.some_id)
LEFT JOIN bar b
ON child_id = parent_id
WHERE ((-b.value)) = 1          -- 3
