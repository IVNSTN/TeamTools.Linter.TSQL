DECLARE @foo TABLE  (
    id int DEFAULT (1)
)

BEGIN DIALOG CONVERSATION @Handle
    FROM SERVICE some_source_svc
    TO SERVICE 'some_target_svc'
    ON CONTRACT my_contract
    WITH
        LIFETIME = 604800
        , ENCRYPTION = OFF;

BEGIN TRY
    PRINT 'aaa';

    ;WITH cte AS (select 'far' as far)
    select (1+2) * GETDATE() as a, b, SUM(c) OVER() from cte
END TRY
BEGIN CATCH
    /*
    comment
    */
    RAISERROR('test', 16, 1)
END CATCH;

RETURN;
