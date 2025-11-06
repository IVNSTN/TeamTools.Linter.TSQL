declare @c cursor;

set @c = cursor fast_forward for
    select 1
    order by 1

declare cr cursor local forward_only static for
    select a, b, (select top 1 z from bar) as t
    from foo
    order by b desc;
