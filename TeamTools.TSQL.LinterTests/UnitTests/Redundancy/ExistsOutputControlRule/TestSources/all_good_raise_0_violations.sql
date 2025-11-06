SELECT foo
from bar
where exists(select 1 from far where far.id = bar.id)
or exists(select 1 from far group by parent_id having sum(volume) > 0)
