SELECT ISNULL(NULL, 1) + 3
FROM c
WHERE d <> NULL           -- 1
    OR e = NULL + 3       -- 2
