-- bare select
SELECT 1

-- no FROM
SELECT 1
WHERE 1=1

-- no WHERE
SELECT *
FROM foo
LEFT JOIN bar
ON child_id = parent_id

-- no JOIN
SELECT *
FROM foo
WHERE foo.group_id = 123

-- WHERE CURRENT OF
UPDATE foo
SET flag = 1
FROM foo
LEFT JOIN bar
ON child_id = parent_id
WHERE CURRENT OF cr

-- no OUTER JOIN
SELECT *
FROM foo
INNER JOIN bar
ON child_id = parent_id
INNER JOIN far
ON child_id = parent_id
INNER JOIN jar
ON child_id = parent_id
WHERE bar.x = foo.y

-- WHERE is not related to join
SELECT *
FROM foo
LEFT JOIN bar
ON id = parent_id
INNER JOIN far
ON far.id = foo.id
WHERE far.x = 1
OR foo.y = 1

-- it's still OUTER
SELECT *
FROM foo
LEFT JOIN bar
ON child_id = parent_id
WHERE bar.id IS NULL

-- ON ON
SELECT *
FROM foo
    LEFT JOIN bar
        INNER JOIN far
            ON far.id = bar.id
    ON bar.parent_id = foo.id
WHERE far.value IS NULL

-- OR
SELECT *
FROM foo f
LEFT OUTER JOIN bar b
ON child_id = parent_id
WHERE b.value IS NOT NULL
OR f.parent_id IS NULL

-- nested WHERE
select *
from foo
inner join
(
    select *
    from bar
    where bar.flag > 1
) b
on b.id = foo.id

select *
from foo
left join
(
    select *
    from bar
    where bar.flag > 1
) b
on b.id = foo.id
where foo.group_id = 123
