set @var = (select 1 from dbo.test_existance)

if exists(select 1 from dbo.t)
    return;

if exists(select 1 from dbo.t
    where (exists(select 1 from dbo.foo)
        or exists(select 2 from dbo.bar))
)
    SELECT TOP(1) 1
    from my_tab
