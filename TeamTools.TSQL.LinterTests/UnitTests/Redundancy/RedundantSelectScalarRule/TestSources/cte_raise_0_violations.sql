WITH cte (db_date) AS
(
    SELECT CAST(CASE
                    WHEN @Date IS NOT NULL THEN
                        @Date
                    WHEN @DateSel = 1
                        AND @Date IS NULL THEN
                        CAST(GETDATE() AS DATE)
                    ELSE
                        NULL
                END AS DATETIME)
)
SELECT * FROM cte;
