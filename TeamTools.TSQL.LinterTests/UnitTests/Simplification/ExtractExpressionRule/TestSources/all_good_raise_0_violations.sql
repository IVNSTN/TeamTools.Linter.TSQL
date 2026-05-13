-- all expressions are different
SELECT
    NULL as id,
    (1 + 1) as num,
    dbo.my_fn(x, y) as z
from foo
inner join bar
on parent_id = some_id - 1
order by id

-- order by should be ignored - there is a separate rule for that
SELECT
    NULL as id,
    (1 + 1) as num
from foo
order by (1 + 1)
