select 1 as id, t.a, null as name
from dbo.foo
union
select 1 as id, t.a, zar.title
from dbo.bar

select 1 as id, t.a, zar.title
from dbo.zar
union all -- id is permanintly different but ALL already enabled
select 0 as id, t.a, zar.title
from dbo.bar
