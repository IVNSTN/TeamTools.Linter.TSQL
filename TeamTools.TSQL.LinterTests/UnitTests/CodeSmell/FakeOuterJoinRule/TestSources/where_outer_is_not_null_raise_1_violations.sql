SELECT *
FROM foo f
LEFT OUTER JOIN bar b
ON child_id = parent_id
WHERE b.value IS NOT NULL
