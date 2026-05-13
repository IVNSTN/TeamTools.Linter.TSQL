select 1
from foo
inner HASH join bar -- here
on id = parent_id
