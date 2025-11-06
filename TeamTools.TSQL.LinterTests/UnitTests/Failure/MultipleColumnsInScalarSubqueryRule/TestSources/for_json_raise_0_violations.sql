-- compatibility level min: 130
SELECT 'a',
    (SELECT a, b, c
    FROM dbo.sub_query
    FOR JSON PATH)
FROM dbo.foo
FOR JSON PATH
GO
