;WITH cte (id, name, dt) as
(
    select 1, null, GETDATE()
)
select * from cte

;WITH cte as
(
    select 1 as id, null as name, GETDATE() as dt
)
select * from cte

select *
from
(
    select 1, null, GETDATE()
) as src (id, name, dt)

select *
from
(
    select 1, null, GETDATE()
) as src (id, name, dt)
inner join
(
    select 1, null, GETDATE()
) as dst (id, name2, dt2)
on dst.id = src.id

select *
from
(
    select 1 as id, null as name, GETDATE() as dt
) as src
inner join
(
    select 1 as id, null as name2, GETDATE() as dt2
) as dst
on dst.id = src.id;

;merge foo as foo
using (
    select 1 as id, null as name, GETDATE() as dt
) as bar
on foo.id = bar.id
WHEN MATCHED THEN DELETE;

;merge foo as foo
using (select 1, null, GETDATE()) as bar (id, name, dt)
on foo.id = bar.id
WHEN MATCHED THEN DELETE;

select *
from foo
CROSS JOIN (SELECT @work_price_end AS a_date UNION SELECT @current_day AS a_date) AS dt(a_date);
