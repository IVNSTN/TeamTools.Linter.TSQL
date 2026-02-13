select 1
from a
inner join @bar as b on b.id = a.parent_id and b.x > b.y
inner join far as f on f.some_id = a.other_id
left join car as c on c.some_id = a.another_id and @var > 100
where a.group_id = 123
    and (((b.x)) > b.y)             -- here

select 1
from a
inner join bar on bar.id = a.parent_id
    and ((123)) = group_id
where (group_id = 123)              -- here
    and bar.y < bar.x
