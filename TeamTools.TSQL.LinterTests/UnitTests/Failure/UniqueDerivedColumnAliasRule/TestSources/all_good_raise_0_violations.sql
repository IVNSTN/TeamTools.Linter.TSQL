select 1 as a from dbo.my_view

select a, b as a -- ignored here
from (select 1, a) as v(a, b)

select a, b as a -- ignored here
from (values (1), (2)) as v(a, b)

;with cte(a, b, c) as
(select 1, 2, 3)
select * from cte

;merge foo as foo
using (
    select 1, null, GETDATE()
    union
    select 2, null, GETDATE()
) as bar (id, name, dt)
on foo.id = bar.id
WHEN MATCHED THEN DELETE;
