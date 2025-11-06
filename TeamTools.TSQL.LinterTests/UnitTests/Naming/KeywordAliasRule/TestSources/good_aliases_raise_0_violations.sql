WITH source_cte AS (
    SELECT * FROM dbo.some_tbl st
)
SELECT
    1 + 1 AS col_name,
    MyColumn = src.another_column
FROM dbo.source_cte AS src
