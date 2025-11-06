SELECT *
from foo
left join bar (HOLDLOCK)
    on bar.id = foo.id
OPTION (FORCE ORDER, OPTIMIZE FOR UNKNOWN);
