merge dst
using src
on 1=1
when matched then
    update set
        a = src.a
        , b = src.b
        , dst.a = src.c
when not matched by target then
    insert (a, a, b)
    values (src.a, src.b, src.b)
when not matched by source then
    delete;
