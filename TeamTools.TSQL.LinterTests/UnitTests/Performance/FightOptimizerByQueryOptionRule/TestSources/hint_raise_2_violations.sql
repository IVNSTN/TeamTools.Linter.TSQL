select 1
from foo
inner join bar
on id = parent_id
OPTION (FORCE ORDER)    -- 1

select 1
from foo
inner join bar
on id = parent_id
OPTION (MAXDOP 0)       -- 2
