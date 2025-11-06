SELECT
    col_a
    , (SELECT zdar FROM far AS fr WHERE fr.id = foo.next_id) AS col_b
FROM foo
INNER JOIN bar AS br
    ON br.x = y
LEFT JOIN t as t
    ON t.id = br.id
WHERE NOT EXISTS (SELECT 1 FROM z AS z WHERE z.id = foo.id)
ORDER BY (SELECT TOP(1) name FROM zar AS zr ORDER BY 1 DESC)
