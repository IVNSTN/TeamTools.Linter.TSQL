UPDATE t SET
    title = new_title -- 1
OUTPUT t.title
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id
WHERE t.title LIKE 'asdf%'
GO

UPDATE t SET
    title = b.new_title
OUTPUT title -- 2
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id
WHERE t.title LIKE 'asdf%'
GO

UPDATE t SET
    title = b.new_title
OUTPUT t.title
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = parent_id -- 3
WHERE t.title LIKE 'asdf%'
GO

UPDATE t SET
    title = b.new_title
OUTPUT t.title
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id
WHERE title LIKE 'asdf%' -- 4
