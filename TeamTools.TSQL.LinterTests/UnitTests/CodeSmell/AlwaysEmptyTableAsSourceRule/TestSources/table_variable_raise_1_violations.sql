DECLARE @foo TABLE (id INT)
CREATE TABLE #test (name varchar(100))

delete o
output deleted.name into #test(name)
from dbo.orders as o
where canceled = 1
and exists(select 1 from @foo)

select * From #test
