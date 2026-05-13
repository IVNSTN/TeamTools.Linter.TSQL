select 1
from foo
inner join bar
on id = parent_id
OPTION (RECOMPILE, MAXRECURSION 10, MAXDOP 2, OPTIMIZE FOR UNKNOWN);
