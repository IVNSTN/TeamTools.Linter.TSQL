IF 1=0
    SELECT HOST_NAME()

UPDATE t SET
    caller = HOST_NAME()
FROM tbl as t
WHERE caller is null

WHILE @a = @b
    DELETE dbo.clients
GO


-- where without predicate
UPDATE #tbl SET
    a=  b
WHERE CURRENT OF cr
