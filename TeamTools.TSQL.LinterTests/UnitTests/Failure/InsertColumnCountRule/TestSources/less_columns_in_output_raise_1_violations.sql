insert into bar(a)
    output inserted.a, inserted.b
    into foo(z, zz, zzz)
select (select 1);
