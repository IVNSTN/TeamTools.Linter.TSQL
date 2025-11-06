;merge foo as foo
using (select 1, null, GETDATE()) as bar (id, dt)
on foo.id = bar.id
WHEN MATCHED THEN DELETE;

;merge foo as foo
using (select 1, null) as bar (id, name, dt, summary)
on foo.id = bar.id
WHEN MATCHED THEN DELETE;
