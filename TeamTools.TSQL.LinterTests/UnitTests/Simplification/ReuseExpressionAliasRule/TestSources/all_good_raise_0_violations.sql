-- no ORDER BY
SELECT 1

-- no selected elements
SELECT @x = y
ORDER BY y

-- simple expression sorted
SELECT
    foo.bar as far
FROM foo
ORDER BY foo.bar ASC

-- expression reused
SELECT
    (foo.bar * foo.jar - 1) as far
FROM foo
ORDER BY far DESC
