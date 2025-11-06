SELECT 'a',
    (SELECT bar
    FROM dbo.sub_query)
FROM dbo.foo
GO

SELECT 'a',
    (SELECT bar
    FROM dbo.sub_query)
FROM dbo.foo
FOR XML AUTO
GO

DECLARE @x INT

SET @x = (SELECT a
    FROM dbo.sub_query)
