EXEC dbo. /* 1 */ foo               -- 1, 2, 3

SELECT t.    col FROM dbo    .tbl   -- 4, 5
GO

CREATE FUNCTION foo/* */.bar()          -- 6
RETURNS dbo.
-- 7, 8, 9
my_type
BEGIN
    RETURN 1
END
