SELECT 1
FROM foo
INNER JOIN bar
ON id = parent_id
OPTION (MAXDOP 1)             -- here
