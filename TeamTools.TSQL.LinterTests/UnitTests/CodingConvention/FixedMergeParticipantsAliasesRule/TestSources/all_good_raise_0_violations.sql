MERGE dbo.foo as trg
USING (select a, b, c from dbo.bar as br) src
on trg.id = src.id
WHEN MATCHED THEN
    DELETE;
