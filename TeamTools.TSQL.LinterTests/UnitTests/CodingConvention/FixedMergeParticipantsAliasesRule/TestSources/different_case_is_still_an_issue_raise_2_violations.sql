MERGE dbo.foo TRG
USING (select a, b, c from dbo.bar as br) Src
on trg.id = src.id
WHEN MATCHED THEN
    DELETE;
