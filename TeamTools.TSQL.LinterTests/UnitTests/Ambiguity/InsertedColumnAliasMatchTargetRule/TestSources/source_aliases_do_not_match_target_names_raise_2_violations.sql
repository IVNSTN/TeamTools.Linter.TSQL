insert into acme(a, b, c, d) -- 1
    output inserted.a, inserted.b, 'asdf'
    into z(x, y, z) -- 2
select @who,
    (select a from bar),
    foo.c,
    null
from zar
