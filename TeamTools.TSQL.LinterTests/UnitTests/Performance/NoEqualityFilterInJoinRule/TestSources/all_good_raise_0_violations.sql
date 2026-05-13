SELECT *
FROM foo
INNER JOIN bar
ON x = y

SELECT *
FROM foo
FULL JOIN bar
ON bar.x = foo.y
OR (bar.z = foo.t)

SELECT *
FROM foo
FULL JOIN bar
ON bar.x BETWEEN 1 AND foo.y
AND (bar.z > @z)
