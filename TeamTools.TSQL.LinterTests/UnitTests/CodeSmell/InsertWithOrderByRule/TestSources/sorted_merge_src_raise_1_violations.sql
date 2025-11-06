MERGE dbo.foo dst
USING (
    SELECT TOP (100) PERCENT *
    FROM dbo.bar
    ORDER BY title
) src
ON id=title_id
WHEN NOT MATCHED BY TARGET THEN
    INSERT(id, title)
    VALUES (src.id, src.title);
