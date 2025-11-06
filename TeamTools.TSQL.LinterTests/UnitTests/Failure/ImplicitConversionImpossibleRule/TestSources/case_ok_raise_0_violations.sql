DECLARE @a INT = 1, @dt DATETIME

SELECT
    CASE
        WHEN (@a = 0) AND (f.id <> 0)
        THEN 3

        WHEN @a < 0
        THEN '5'

        WHEN @a = 1
        THEN GETDATE() -- this is the type with highest precedence, all other THEN types can be converted to DATETIME

        ELSE (SELECT 1*0)
    END
FROM foo AS f

SELECT
    CASE @dt
        WHEN 1 -- int can be converted to date
        THEN 3

        WHEN '0' -- varchar can be converted to date
        THEN '5'

        WHEN GETDATE()
        THEN 7

        ELSE NULL
    END
FROM foo AS f
