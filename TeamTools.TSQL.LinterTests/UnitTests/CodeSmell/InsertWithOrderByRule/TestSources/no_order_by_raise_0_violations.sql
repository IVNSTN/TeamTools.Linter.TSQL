INSERT dbo.foo(id, title)
SELECT id, title
FROM dbo.bar


MERGE dbo.foo dst
USING (SELECT * FROM dbo.bar) src
ON id=title_id
WHEN NOT MATCHED BY TARGET THEN
    INSERT(id, title)
    VALUES (src.id, src.title);
