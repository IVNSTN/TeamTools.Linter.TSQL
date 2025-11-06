select @who as a,
    (select a from bar) as b,
    foo.c,
    null as d
from zar
where exists(select 1 from far)

select  count(1) as x
from b
INNER JOIN
(
    SELECT 3 * 3 as cube
    union all
    select 4 * 4 as square
) f
ON id = id
group by c
having sum(d) = 0
order by (select 1)

set @bars = (select count(0) from bar)
select @nar = jar
from mar
where exists (select 1 from tbl)
