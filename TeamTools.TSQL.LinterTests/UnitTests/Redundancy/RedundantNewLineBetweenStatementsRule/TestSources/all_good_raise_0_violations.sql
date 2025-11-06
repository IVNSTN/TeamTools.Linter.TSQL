DECLARE @foo TABLE  (
    id int DEFAULT (1)
)

IF 1 = 0
BEGIN
    PRINT 'aaa';

    ;WITH cte AS (select 'far' as far)
    select (1+2) * GETDATE() as a, b, SUM(c) OVER() from cte
END;
ELSE
BEGIN
    /*
    comment
    */
    RAISERROR('test', 16, 1)
END;

RETURN;
