select *
from foo
left join bar
    on bar.id = foo.id

select *
from foo
left join bar WITH(INDEX=asf, HOLDLOCK)
    on bar.id = foo.id
OPTION (FORCE ORDER, OPTIMIZE FOR UNKNOWN);
