merge dbo.foo dst
using dbo.bar src
on src.id = dst.id
WHEN NOT MATCHED BY TARGET THEN
    insert
    values (a, b, c);
