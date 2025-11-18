DECLARE @foo TABLE
(
    id int DEFAULT (1)
)

;WITH cte AS (select 'far' as far)
select (1+2) * GETDATE() as a, b, SUM(c) OVER() from cte

select *
from
(
    select
        case when
            CASE WHEN (a = b) THEN 1 ELSE 0 END = 1
            THEN SYSDATETIME()
            else
                getdate()
        end as val,
        (
            1+1
        ) as val2
) t
