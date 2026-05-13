SELECT
    foo.title,
    foo.start_time
FROM foo

UNION ALL

-- types are provided by the first part in union
SELECT
    ''
    , NULL
FROM bar
