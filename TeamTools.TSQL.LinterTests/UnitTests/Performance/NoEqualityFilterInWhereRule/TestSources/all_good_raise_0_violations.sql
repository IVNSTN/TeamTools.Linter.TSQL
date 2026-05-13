-- no where
SELECT *
FROM foo
INNER JOIN bar
ON x = y

-- no from
SELECT *
WHERE @a > 1

-- temp tables are ignored
SELECT *
FROM @foo
WHERE value > 1

SELECT *
FROM #foo
WHERE x <= z

-- good filter
SELECT *
FROM foo
INNER JOIN bar
ON x = y
WHERE bar.date_year = @year

SELECT *
FROM foo
INNER JOIN bar
ON x = y
WHERE bar.name LIKE 'asdf%'

SELECT *
FROM foo
INNER JOIN bar
ON x = y
WHERE bar.parent_id IN (1, 2, @three)
