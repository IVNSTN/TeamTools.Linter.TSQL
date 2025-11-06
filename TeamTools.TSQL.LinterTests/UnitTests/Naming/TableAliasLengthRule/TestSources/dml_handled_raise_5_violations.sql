delete t from tbl as t
inner join fk as f
on f.id=  t.id

update accounts set 
    a = NULL
from bills b
where b.acc_id= accounts.id

insert dest
select x.*
from extracted x
cross join distracted d
