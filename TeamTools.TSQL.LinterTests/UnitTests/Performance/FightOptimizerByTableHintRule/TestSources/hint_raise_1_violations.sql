select 1
from foo WITH (FORCESEEK)  -- here
inner join bar
on id = parent_id
