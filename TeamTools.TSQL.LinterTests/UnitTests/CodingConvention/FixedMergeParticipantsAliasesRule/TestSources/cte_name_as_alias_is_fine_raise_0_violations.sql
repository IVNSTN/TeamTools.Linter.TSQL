WITH src AS (select a, b, c from dbo.bar as br),
trg as (select * from foo)
MERGE trg
USING src
on trg.id = src.id
WHEN MATCHED THEN
    DELETE;
