UPDATE sa WITH(HOLDLOCK)
SET dt_last_update = DEFAULT
    output deleted.z, inserted q into dbo.acme(x, w)
FROM some_accounts AS sa
cross apply (
    select 1 from dbo.foo join dbo.bar as bb
    on dbo.foo.x = bb.y
    where sa.fa = dbo.foo.ma
) as zz
WHERE sa.disabled = 'N';

UPDATE f WITH(TABLOCK)
SET last_mod = SYSDATETIME()
FROM foo f
inner join bar b
    on b.id = f.id;
