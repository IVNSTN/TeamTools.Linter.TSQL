MERGE dbo.foo AS target
USING (select 1 as id) AS source
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id) VALUES (source.id);

SET @id = SCOPE_IDENTITY();
