SELECT a, b, sum(c)
from foo
group by a, b
having
    (
        max((a)) > 0
        or CASE WHEN b=c THEN c ELSE b END IN (1, 2)
    )
    and not (a > b)
    and ((a is null or not 0 = -e))
