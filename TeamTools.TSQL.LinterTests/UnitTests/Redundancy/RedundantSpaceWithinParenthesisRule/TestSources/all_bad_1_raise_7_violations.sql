SELECT GETDATE( ) -- 1

-- here spaces after linebreak fine
DECLARE @foo TABLE  (
    id int DEFAULT ( 1 ) -- 2
)

;WITH cte AS ( select 'far' as far) -- 3
select (1+2  ) *  -- 4
    -- here not fine
    GETDATE(
    ) as a, b,    -- 5
    SUM( ( c ) ) OVER( ) -- 6, 7
from cte
