-- compatibility level min: 110
SELECT foo
from bar
where exists(select 1
from far 
where far.id = bar.id
group by title
order by title
offset 10 rows fetch next 10 rows only)
