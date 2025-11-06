DECLARE @a INT = 1, @dt DATETIME, @uid UNIQUEIDENTIFIER

SELECT
    CASE
        WHEN (@a = 0) AND (f.id <> 0)
        THEN 3

        WHEN @a < 0
        THEN '5'

        WHEN @a = 1
        THEN CAST(GETDATE() AS DATETIME2) -- this is the type with highest precedence, none other THEN types can be converted to DATETIME

        ELSE (SELECT 1*0)
    END
FROM foo AS f

SELECT
    CASE @dt
        WHEN 1
        THEN 3

        WHEN '0'
        THEN '5'

        WHEN @uid -- GUID is not compatible with DATETIME
        THEN 7

        ELSE NULL
    END
FROM foo AS f
