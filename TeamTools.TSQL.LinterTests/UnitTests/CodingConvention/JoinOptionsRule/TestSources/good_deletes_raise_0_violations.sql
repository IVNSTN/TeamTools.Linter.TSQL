DELETE dbo.foo WHERE 1=0;

DELETE sa WITH(ROWLOCK)
    output deleted.z into dbo.acme(x)
FROM some_accounts AS sa
JOIN dbo.acc_access on acc_access.acc_code = sa.acc_code
    AND sa.blocked = 'N'
cross apply (
    select 1 from dbo.foo join dbo.bar as bb
    on dbo.foo.x = bb.y
    where sa.fa = dbo.foo.ma
) as zz;

DELETE fa WITH(ROWLOCK)
    OUTPUT deleted.xx
    INTO dbo.acme(x)
FROM dbo.far AS fa
INNER JOIN dbo.jar ON jar.rec_code = fa.rec_code
    AND jar.need_update = 'Y';
GO
