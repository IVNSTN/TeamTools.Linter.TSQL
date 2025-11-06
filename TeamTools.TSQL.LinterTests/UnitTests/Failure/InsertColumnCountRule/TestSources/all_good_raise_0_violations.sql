declare @who varchar(10) = 'giraffe'

insert into bar(a, walks, c, d)
values (@who, 1, (select top 1 1 from foo), NULL),
    (DEFAULT, DEFAULT, DEFAULT, DEFAULT);

insert into acme(a, b, c, d)
    output inserted.a, inserted.b, 'asdf'
    into z(y, c, f)
select @who as walks,
    (select a from bar),
    foo.x,
    null
from foo;

merge dbo.foo dst
using dbo.bar src
on src.id = dst.id
WHEN NOT MATCHED BY TARGET THEN
    insert (aa, bb, cc)
    values (a, b, c);
