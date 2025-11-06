WITH [RETURN] AS (
    SELECT * FROM dbo.some_tbl
)
SELECT
    1 + 1,
    another_column
FROM dbo.source_cte AS INT;
