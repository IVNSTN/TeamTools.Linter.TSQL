-- no from
select 1
where 1=0

-- no where
select 1
from a

update t set
    x = y
from a
WHERE CURRENT OF cr

-- no join
select 1
from a
where x > y

-- no inner join
select 1
from a
left join b on id = parent_id
outer apply dbo.my_fn(arg)
right join c on foo = bar
where x > y

-- no dups
select 1
from a
inner join b on b.id = a.parent_id and b.x > b.y
where a.group_id = 123
and b.value is not null

