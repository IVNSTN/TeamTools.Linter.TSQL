DECLARE @foo TABLE (id INT)
CREATE TABLE #test (name varchar(100))

insert @foo values (123)

delete o
from dbo.orders as o
where canceled = 1
and exists(select 1 from @foo)

select * from #test
