

DECLARE @foo TABLE  (
    id int DEFAULT (1)
)



IF 1 = 0


BEGIN

    PRINT 'aaa';
END

ELSE

    PRINT 'bbb';


;WITH cte AS (select 'far' as far)
select (1+2) * GETDATE() as a, b, SUM(c) OVER() from cte

/*
comment
*/
RAISERROR('test', 16, 1)


RETURN 0;
