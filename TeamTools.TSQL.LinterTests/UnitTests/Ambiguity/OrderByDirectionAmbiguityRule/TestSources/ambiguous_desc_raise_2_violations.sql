SELECT
    id,
    ROW_NUMBER() OVER (ORDER BY a, b DESC) as rn
FROM foo
ORDER BY d DESC, e
