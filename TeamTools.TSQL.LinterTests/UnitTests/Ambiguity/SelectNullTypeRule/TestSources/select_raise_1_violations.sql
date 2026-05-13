SELECT
    foo.title,
    (SELECT (NULL)) as start_time  -- here
FROM foo
