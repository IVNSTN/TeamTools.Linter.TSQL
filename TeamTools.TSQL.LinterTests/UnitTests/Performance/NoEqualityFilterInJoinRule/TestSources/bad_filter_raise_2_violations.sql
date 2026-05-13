SELECT *
FROM foo
INNER JOIN bar
ON x > y

SELECT *
FROM foo
FULL JOIN bar
ON @a = 1
OR bar.value IS NOT NULL
