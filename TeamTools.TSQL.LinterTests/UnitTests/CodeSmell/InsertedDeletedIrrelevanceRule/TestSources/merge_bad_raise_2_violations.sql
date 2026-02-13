merge foo
using bar
on a = b
when matched then
    delete
output inserted.*   -- 1
;

merge foo
using bar
on a = b
when not matched by target then
    insert (x, y)
    values (z, w)
output deleted.a   -- 2
into #tmp (deleted_a)
;
