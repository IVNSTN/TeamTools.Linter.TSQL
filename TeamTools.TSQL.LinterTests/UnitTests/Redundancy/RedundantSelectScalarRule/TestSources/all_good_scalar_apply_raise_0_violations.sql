SELECT
    t.*,
    row_number() 
    over(order by (select null)) rn -- must be ignored
from tbl
OUTER APPLY
(
    SELECT CASE WHEN
               tbl.volume IS NOT NULL
                  AND
                  (
                      EXISTS (SELECT 1 FROM foo.bar WHERE date > GETDATE())
                      OR EXISTS (SELECT 1 FROM bar.foo WHERE volume < 100)
                  )
              THEN 'Y'
              ELSE 'N'
              END AS flag
) AS calc;

select *
from nums
CROSS APPLY (SELECT DATEADD(DAY, nums.n, @date_begin)) AS cal(dt)
