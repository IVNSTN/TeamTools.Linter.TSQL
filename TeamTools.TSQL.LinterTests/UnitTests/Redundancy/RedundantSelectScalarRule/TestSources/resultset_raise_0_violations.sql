select @var as id
select @var
select GETDATE()

select 'USD' as currency
select @var, 'USD'

select 1, t.name
from dbo.foo

;with cte As (
select 1 as id
) select *, 2 from cte
