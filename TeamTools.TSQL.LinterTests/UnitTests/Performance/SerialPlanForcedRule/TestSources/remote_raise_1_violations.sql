SELECT 1
FROM foo
INNER REMOTE JOIN bar       -- here
ON id = parent_id
