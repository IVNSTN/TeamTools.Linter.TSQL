select t.*, old.id as name, new.name as id
from dbo.tbl as t
inner join t as old
    on 1=1
inner join old as new
    on 1=1
