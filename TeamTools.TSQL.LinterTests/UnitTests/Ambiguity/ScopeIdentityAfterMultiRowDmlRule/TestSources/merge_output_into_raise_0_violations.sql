MERGE dbo.foo AS target
USING dbo.src AS source
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id) VALUES (source.id)
OUTPUT INSERTED.id INTO @ids;
