DECLARE @foo TABLE  (
    id int DEFAULT (1)
)

SELECT ((1) - (2)) as math

SET @Subject = CAST((@Counter - 1) AS VARCHAR(10));

;WITH cte AS (select 'far' as far)
select (1+2) * GETDATE() as a, b, SUM(c) OVER() from cte

SELECT @ErrMess =
    OBJECT_SCHEMA_NAME(@@PROCID) + '.' + OBJECT_NAME(@@PROCID) + '.' + ISNULL('err_num = ' + CONVERT(VARCHAR(30), ERROR_NUMBER()) + '.', '')
    + ISNULL('state = ' + CONVERT(VARCHAR(30), ((ERROR_STATE()))) + '.', '') + ISNULL(ERROR_MESSAGE(), '')
