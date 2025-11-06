DECLARE
    @date        DATE
    , @time      TIME
    , @datetime DATETIME
    , @datetime2 DATETIME2
    , @ts        TIMESTAMP;

SET @date = SYSDATETIME();
SET @time = '12:00';

SELECT @datetime2 = CAST(@date AS DATETIME2(7))

SET @datetime += 1; -- 4
SET @datetime = CURRENT_TIMESTAMP;
