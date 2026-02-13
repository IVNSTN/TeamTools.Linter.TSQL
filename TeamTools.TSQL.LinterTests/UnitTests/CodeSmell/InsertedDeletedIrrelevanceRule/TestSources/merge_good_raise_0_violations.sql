merge foo
using bar
on a = b
when matched then
    delete
-- no output
;

merge foo
using bar
on a = b
when matched then
    delete
output deleted.*;

merge foo
using bar
on a = b
when not matched by target then
    insert (x, y)
    values (z, w)
output $action, inserted.a
into #tmp (act, inserted_a);

merge foo
using bar
on a = b
when matched then
    delete
when not matched by target then
    insert (x, y)
    values (z, w)
output inserted.a, deleted.w
into #tmp (inserted_a, deleted_w);

merge foo
using bar
on a = b
when matched then
    update set
        title = new_title
output inserted.title as new_title, deleted.title as old_title;
