SELECT *
FROM foo
INNER JOIN bar
ON x = y
WHERE bar.date > @year_begin

SELECT *
FROM foo
LEFT JOIN bar
ON x = y
WHERE bar.flag IS NOT NULL
