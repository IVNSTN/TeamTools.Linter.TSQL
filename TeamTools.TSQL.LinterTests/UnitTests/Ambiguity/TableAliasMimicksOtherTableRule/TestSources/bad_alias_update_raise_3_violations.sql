update old set
    name = new.name
from dbo.tbl as t
inner join t as old         -- 1
    on 1=1
inner join old as new       -- 2
    on 1=1
cross apply same as same
cross join another as tbl   -- 3
