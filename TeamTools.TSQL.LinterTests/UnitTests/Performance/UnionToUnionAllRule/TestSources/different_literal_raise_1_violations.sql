select 1 as id, t.a, null as name
from dbo.foo
union
select 0 as id, t.a, zar.title -- id is always different
from dbo.bar
