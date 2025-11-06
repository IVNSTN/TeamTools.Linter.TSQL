DECLARE @foo TABLE  (
    id int DEFAULT 1
)

;WITH cte AS (select 'far' as far)
select (1+2) * GETDATE() as a, b, SUM(c) OVER() from cte


SELECT TOP(1) foo,
    ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS rn,
    COUNT(1) OVER(PARTITION BY goo ORDER BY zoo) AS cnt
FROM bar
