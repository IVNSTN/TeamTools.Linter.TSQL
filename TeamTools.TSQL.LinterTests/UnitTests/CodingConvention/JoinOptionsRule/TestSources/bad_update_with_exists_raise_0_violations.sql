DELETE dbo.foo WITH(ROWLOCK)
WHERE not exists(select 1 from dbo.bar WHERE bar.id = foo.id);
