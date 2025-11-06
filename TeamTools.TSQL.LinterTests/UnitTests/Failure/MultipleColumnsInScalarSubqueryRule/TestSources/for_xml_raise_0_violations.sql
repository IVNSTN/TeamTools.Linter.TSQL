-- compatibility level min: 110
SELECT 'a',
    (SELECT a, b, c
    FROM dbo.sub_query
    FOR XML PATH)
FROM dbo.foo
GO

DECLARE @x XML

SET @x = (SELECT a, b, c
    FROM dbo.sub_query
    FOR XML PATH)
