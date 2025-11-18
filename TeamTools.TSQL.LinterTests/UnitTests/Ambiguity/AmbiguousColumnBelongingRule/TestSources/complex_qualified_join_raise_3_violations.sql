SELECT a.title,
    b.id
FROM dbo.aaa AS a
INNER JOIN
(
    SELECT id, title, dt --- from bbb or ccc?
    FROM dbo.bbb
    INNER JOIN dbo.ccc
        ON ccc.parent_id = bbb.child_id
) AS b
ON b.id = a.id
