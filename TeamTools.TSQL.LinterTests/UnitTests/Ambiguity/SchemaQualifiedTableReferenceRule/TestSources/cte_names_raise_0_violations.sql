WITH cte as(select * from dbo.mytbl)
SELECT id
FROM (SELECT * from cte) t
