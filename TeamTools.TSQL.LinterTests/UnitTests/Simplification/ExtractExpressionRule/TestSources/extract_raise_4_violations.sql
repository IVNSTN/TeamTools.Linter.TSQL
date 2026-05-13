SELECT
    NULL as id,
    (1 + 2 * 3) as num,
    dbo.my_fn(x, y) as z,
    (dbo.my_fn(x, y)) as pin    -- 1
from foo
inner join bar
on parent_id = dbo.my_fn(x, y)  -- 2
and volume > (1 + 2 * 3)        -- 3
group by dbo.my_fn(x, y)        -- 4
