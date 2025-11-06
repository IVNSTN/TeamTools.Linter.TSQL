SELECT foo
from bar
where exists(select distinct top (100) title, volume, dtinsert
from far 
where far.id = bar.id
order by title)
