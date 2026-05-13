SELECT TOP(100)                 -- 1
    ROW_NUMBER()                -- 2
    OVER(ORDER BY parent_id)
FROM foo
INNER LOOP JOIN bar             -- 3
ON id = parent_id
OPTION (MAXRECURSION 10)        -- 4
