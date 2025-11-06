MERGE dbo.foo
USING (select a, b, c from dbo.bar as br) s
on foo.id = s.id
WHEN MATCHED THEN
    DELETE;
