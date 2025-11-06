MERGE dbo.foo AS f
USING
(
    SELECT id, title
    FROM dbo.bar
) AS b
ON f.id = b.id
WHEN MATCHED THEN
    UPDATE SET title = b.title
WHEN NOT MATCHED THEN
    INSERT (id, title)
    VALUES
    (b.id, b.title);
