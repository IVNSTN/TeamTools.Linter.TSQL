IF @A = NULL              -- 1
    SELECT (NULL) + 3       -- 2
    FROM c

WHILE (@e != null)        -- 3
BEGIN
    PRINT 1
END

select 1 + (select (NULL)) * 2 -- 4
