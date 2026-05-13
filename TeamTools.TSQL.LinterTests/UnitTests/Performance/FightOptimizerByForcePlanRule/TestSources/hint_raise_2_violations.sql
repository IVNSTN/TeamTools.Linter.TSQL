SET FORCEPLAN ON               -- 1

select 1
from foo
inner join bar
on id = parent_id
OPTION (USE PLAN '<xml/>')      -- 2
