insert foo(a, b, c)
    output inserted.a, b, c
    into bar
select a, b, c
from bar

insert dbo.foo
values (1, 2, 3)

update t set
    a = 1
    , t.b = 2
    output inserted.a, b, c
    into bar(a, b, c)
from tbl as t

merge dst
using src
on 1=1
when matched then
    update set
        a = src.a
        , dst.b = src.b
when not matched by target then
    insert (a, b)
    values (src.a, src.b)
when not matched by source then
    delete
    output inserted.a, b, c
    into bar(a, b, c);
