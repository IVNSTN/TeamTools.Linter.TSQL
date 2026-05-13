-- fine ISNULL
select *
from foo
where col > ISNULL(@arg, 1)

-- no ISNULL
select *
from foo
where col > @arg

-- equality is allowed
select *
from foo
where col >= ISNULL(@arg, col)
