insert into dbo.foo(a)
    output inserted.a
    into dbo.acme(a)
select 1 from dbo.bar;
