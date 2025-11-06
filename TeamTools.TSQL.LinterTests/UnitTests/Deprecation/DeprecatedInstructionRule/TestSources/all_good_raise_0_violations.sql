select *
from foo
CROSS JOIN (SELECT @work_price_end AS a_date UNION SELECT @current_day AS a_date) AS dt(a_date);

grant select on dbo.tbl to public

select a from b
group by a;

drop index idx on dbo.tbl;


SELECT *
FROM dbo.foo
FOR XML EXPLICIT, ROOT('r')
