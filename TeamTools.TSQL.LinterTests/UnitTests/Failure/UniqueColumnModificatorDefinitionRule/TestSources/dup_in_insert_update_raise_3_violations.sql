insert foo(a, a, c)
select a, b, c
from bar

insert foo(a, a, c)
values (0, 0, 1);

update t set
    t.a = 1
    , b = 2
    , a = 3
from tbl as t
