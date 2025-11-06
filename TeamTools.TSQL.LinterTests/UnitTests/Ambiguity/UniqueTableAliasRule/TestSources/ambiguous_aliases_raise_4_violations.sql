UPDATE t SET a = col_b
FROM zzz
CROSS APPLY (
    SELECT TOP(100)
        col_a
        , (SELECT zdar FROM far AS t WHERE t.id = foo.next_id) AS col_b -- 1
    FROM foo as foo
    INNER JOIN bar AS t
        ON t.x = y
    CROSS JOIN t -- 2
    WHERE NOT EXISTS (SELECT 1 FROM z AS t WHERE t.id = foo.id) -- 3
    ORDER BY (SELECT TOP(1) name FROM zar AS t ORDER BY 1 DESC) -- 4
) t
