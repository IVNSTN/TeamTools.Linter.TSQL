select *
from dbo.foo

select *
from dbo.foo
order by 1

select top 10 a
from dbo.foo
order by 1

select top (10) a
from dbo.foo
order by a

select * from
(
    select top (100) percent a
    from dbo.foo
    order by CASE WHEN 1=0 THEN 1 ELSE 0 END
) t
