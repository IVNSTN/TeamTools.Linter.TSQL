SELECT id, name
FROM foo f
INNER JOIN bar b
ON f.id = b.id

UNION ALL

-- starting from here
SELECT 1, 'asdf'

UNION ALL

SELECT 2, 'qwer'
