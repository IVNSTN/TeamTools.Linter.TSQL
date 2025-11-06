DECLARE
    @date        DATE
    , @time      TIME
    , @datetime2 DATETIME2
    , @ts        TIMESTAMP;

SET @date = @time; -- 1
SET @time = @date; -- 2

SELECT @datetime2 = (@date + (@time)); -- 3 (plus)

SET @datetime2 = 1; -- 4
SET @datetime2 += @ts; -- 5
