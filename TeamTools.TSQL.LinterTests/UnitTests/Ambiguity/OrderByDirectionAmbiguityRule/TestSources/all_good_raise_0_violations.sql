SELECT
    id,
    ROW_NUMBER() OVER (ORDER BY a ASC, b DESC) as rn
FROM foo
ORDER BY d DESC, e ASC

SELECT
    id,
    ROW_NUMBER() OVER (ORDER BY a ASC, b) as rn
FROM foo
ORDER BY d, e
