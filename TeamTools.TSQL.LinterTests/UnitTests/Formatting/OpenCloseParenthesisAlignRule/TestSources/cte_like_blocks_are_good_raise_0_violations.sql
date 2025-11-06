;WITH cte AS
(
select 'far' as far
)
select 
    (CASE WHEN (1+1 = 2) THEN 3
        ELSE GETDATE() END) as calc
