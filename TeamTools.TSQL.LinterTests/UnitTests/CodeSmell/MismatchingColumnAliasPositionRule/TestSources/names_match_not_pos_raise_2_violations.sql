WITH cte (c, a, b) AS ( -- here
    SELECT a, b, c
    FROM dbo.foo
)
SELECT *
FROM cte c
INNER JOIN (
    SELECT e AS everyone, f AS forgives, g AS gendalf
    FROM dbo.bar
) b (gendalf, forgives, everyone) -- and here
ON b.everyone = c.b
