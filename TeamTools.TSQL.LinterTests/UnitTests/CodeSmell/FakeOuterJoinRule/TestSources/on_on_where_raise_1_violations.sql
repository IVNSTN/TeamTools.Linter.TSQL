SELECT *
FROM foo
    LEFT JOIN bar
        INNER JOIN far
            ON far.id = bar.id
    ON bar.parent_id = foo.id
INNER JOIN jar
on foo.name = jar.name
WHERE far.value > 0             -- here

SELECT *
FROM foo
    LEFT JOIN @bar as bar
    ON bar.parent_id = foo.id
INNER JOIN far
    ON far.id = bar.id
WHERE far.value > 0
