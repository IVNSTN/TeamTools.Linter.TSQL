SELECT 'a',
    (SELECT a, b, c
    FROM dbo.sub_query) -- 1
FROM dbo.foo
GO

SELECT 'a',
    (SELECT a, b, c
    FROM dbo.sub_query) -- 2
FROM dbo.foo
FOR XML AUTO
GO

DECLARE @x XML

SET @x = (SELECT a, b, c
    FROM dbo.sub_query) -- 3
