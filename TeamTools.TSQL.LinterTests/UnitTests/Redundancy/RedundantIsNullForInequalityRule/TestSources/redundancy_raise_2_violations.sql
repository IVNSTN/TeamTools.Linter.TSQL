select *
from foo
where foo.col <> ISNULL(@arg, foo.col)

select *
from foo
where (ISNULL(@arg, foo.col)) > (foo.col)
