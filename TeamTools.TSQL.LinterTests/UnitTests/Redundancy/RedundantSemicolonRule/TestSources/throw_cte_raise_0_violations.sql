-- compatibility level min: 110
;THROW 50001, 'bar', 0

;WITH cte AS (select 'far' as far)
select * from cte
