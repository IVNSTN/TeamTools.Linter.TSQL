MERGE dbo.foo AS target
USING (SELECT 1 AS id UNION ALL SELECT 2) AS source
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id)
    VALUES (source.id);

SET @id = SCOPE_IDENTITY();
