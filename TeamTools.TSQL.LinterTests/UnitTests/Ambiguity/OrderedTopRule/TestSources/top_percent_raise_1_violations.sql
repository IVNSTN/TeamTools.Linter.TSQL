select top 100 percent *, (select TOP 1 1 from bar order by 1) as z
from dbo.foo
