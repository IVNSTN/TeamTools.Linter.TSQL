SELECT 1
FROM dbo.foo
WHERE
    col_a IN ('a', colb, NULL)          -- 1
    OR NULL IN (col_a, colb)            -- 2
    AND colb NOT IN (NULL, 1, 2, 3)     -- 3
