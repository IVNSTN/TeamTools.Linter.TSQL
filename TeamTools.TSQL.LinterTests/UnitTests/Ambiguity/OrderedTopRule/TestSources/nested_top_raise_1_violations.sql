select a, (select top (1) zar from dbo.bar) as z
from dbo.foo
order by a, z
