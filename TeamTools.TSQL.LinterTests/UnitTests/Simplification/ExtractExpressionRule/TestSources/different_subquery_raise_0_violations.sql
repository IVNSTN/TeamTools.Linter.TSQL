SELECT num1, num2
FROM
(
    SELECT (1 + 2 * value) as num1
    FROM foo
) as foo
CROSS JOIN
(
    SELECT (1 + 2 * value) as num2
    FROM bar
) as bar
