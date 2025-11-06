;merge foo as foo
using (
    select 1, null, GETDATE()
    union
    select 2, null, GETDATE()
) as bar (id, id)
on foo.id = bar.id
WHEN MATCHED THEN DELETE;
