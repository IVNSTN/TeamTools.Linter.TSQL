SELECT a, b, sum(c)
from foo
group by a, b
having max(a) > 0

SELECT a, b, sum(c)
from foo
group by a, b
having b > 0
