SELECT a.title,
    b.id
FROM dbo.aaa AS a
INNER JOIN
(
    SELECT id, title, dt
    FROM dbo.bbb --- bbb canno access outside scope by design
) AS b
ON b.id = a.id
