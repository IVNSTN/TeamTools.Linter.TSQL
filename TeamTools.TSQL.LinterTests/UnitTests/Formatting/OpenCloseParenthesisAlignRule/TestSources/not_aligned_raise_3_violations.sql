DECLARE @foo TABLE  (
    id int DEFAULT ( 1 )
)

;WITH cte AS (
select 'far' as far
)
select (1+2  ) *
    GETDATE() as a, b,
    SUM
 ( ( c
   )
        ) OVER( )
from cte
