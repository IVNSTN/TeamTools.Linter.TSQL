SELECT *
from foo
left join bar WITH(HOLDLOCK INDEX(idx))
    on bar.id = foo.id
OPTION (FORCE ORDER, OPTIMIZE FOR UNKNOWN);
