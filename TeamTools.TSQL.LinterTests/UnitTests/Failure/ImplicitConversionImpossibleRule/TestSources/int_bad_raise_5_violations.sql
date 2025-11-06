DECLARE
    @int    INT = GETDATE() -- 1
    , @date DATE
    , @time TIME;

SET @int = @date; -- 2

SELECT @int = @time; -- 3

SELECT 'A'
WHERE 100 > CAST(GETDATE() AS DATETIME2) -- 4
OR @int + 5 <> NEWID() -- 5
