-- ignored
insert into beaver(walks, into_a, bar)
values (@who, 1, (select top 1 1 from foo), NULL);

-- ignored
insert into foo
SELECT 'a', 'b', 'c';

-- fine
insert into acme(a, b, c, d)
    output inserted.a as x, inserted.b as y, 'asdf' as z
    into z(x, y, z)
select @who as a,
    (select a from bar) as b,
    foo.c,
    null as d
from zar
