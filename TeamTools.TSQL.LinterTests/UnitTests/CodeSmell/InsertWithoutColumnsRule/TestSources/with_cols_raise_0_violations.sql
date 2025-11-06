insert into dbo.foo(a, b, c)
select a, b, c
from tmp.bar;

update t set
    a = b
    output inserted (a, b)
    into tmp.bar (a, b)
from dbo.foo as t;

merge dbo.foo dst
using dbo.bar src
on src.id = dst.id
WHEN NOT MATCHED BY TARGET THEN
    insert (aa, bb, cc)
    values (a, b, c);
