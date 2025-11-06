select *
from (values ((select 1 from dbo.foo), (select 2 from dbo.bar))) as v (foo, bar)
