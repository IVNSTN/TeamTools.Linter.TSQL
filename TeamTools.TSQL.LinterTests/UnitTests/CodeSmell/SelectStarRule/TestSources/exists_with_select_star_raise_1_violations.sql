select a
from dbo.foo
where exists(select * from bar)
