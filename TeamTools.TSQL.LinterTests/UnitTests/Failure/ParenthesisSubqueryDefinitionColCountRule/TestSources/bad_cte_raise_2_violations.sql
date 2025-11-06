;WITH cte (id, dt) as 
(
    select 1, null, GETDATE(), 123
)
select * from cte

;WITH cte (id, dt, name, summary) as 
(
    select 1, GETDATE()
)
select * from cte
