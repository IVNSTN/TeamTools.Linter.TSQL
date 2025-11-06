DECLARE @foo TABLE  (
    id int DEFAULT (((1)))  -- +3
)

;WITH cte AS (select 'far' as far)
select ((1+2)) *            -- +1
    (GETDATE()) as a, b,    -- +1
    SUM((c)) OVER()         -- +1
from cte
