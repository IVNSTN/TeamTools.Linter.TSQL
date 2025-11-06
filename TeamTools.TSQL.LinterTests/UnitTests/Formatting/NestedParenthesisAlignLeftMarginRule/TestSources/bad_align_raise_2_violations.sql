
DECLARE @foo TABLE  (
    id int 
DEFAULT (1)  -- 1
            )

SET @Subject = CAST((@Counter - 1) AS VARCHAR(10));

;WITH cte AS (select 'far',
    (1+1)
    as far
)
select (1+2) * GETDATE() as a, b, SUM(
(c)) OVER() from cte -- 2
