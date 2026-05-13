INSERT INTO @foo(title)     -- 1
SELECT title
FROM bar

DELETE @foo                 -- 2
FROM bar

UPDATE @foo SET             -- 3
    is_archived = 1
FROM bar
WHERE dt < @range_start
and bar.id = @foo.parent_id
