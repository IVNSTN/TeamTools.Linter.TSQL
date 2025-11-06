SELECT
    TRIM(@var, 'x'),
    OBJECT_ID('asdf', 'P'),
    OBJECT_NAME(123),
    OBJECT_NAME(123, DB_ID('another db')),
    CONCAT(@var, 'asdf', ' ')
