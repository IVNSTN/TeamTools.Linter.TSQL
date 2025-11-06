SELECT a, b, sum(c)
from foo
group by a, b
having sum(c) > 0 and count(*) > 0

select 1
from a

select c, count(*)
from a
group by c
