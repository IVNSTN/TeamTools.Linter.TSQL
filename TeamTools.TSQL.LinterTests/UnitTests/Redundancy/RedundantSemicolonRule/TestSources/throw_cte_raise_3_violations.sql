-- compatibility level min: 110
BEGIN;
    ;THROW 50001, 'bar', 0;;
END;

;WITH cte AS (select 'far' as far)
select * from cte
