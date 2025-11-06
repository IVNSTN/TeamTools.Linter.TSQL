declare @t table (z int)

select 1 as foo;

select * from acme.bar;

select * from acme.bar where not exists(select 1 from zar);

;with gar as (
    select 'far' as mar
)
select *
from foo
inner join @t
on t.z = foo.b
cross apply (
select zar from bar
) as z
cross join gar

declare @ret_code int, @ret_msg varchar(10);
SELECT @ret_code = 0, @ret_msg = 'Ok';

select 'aa' as asdf;

select *
from (values (1)) t (v);

select *
FROM OPENJSON('dummy');
