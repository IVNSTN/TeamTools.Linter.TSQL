;with cte (id, name) as (
select id * 2, ISNULL(name)
from tbl
)
merge cte t
using (
select 1 + 1, 10 / 10
) as s (id, name)
on t.id= s.id
when matched then
delete;


SELECT ccc FROM dbo.foo
CROSS APPLY
(
    SELECT @bar WHERE a = b
    UNION ALL
    SELECT 3
) AS far(jar)
