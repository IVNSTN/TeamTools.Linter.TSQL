-- compatibility level min: 130
SELECT *
FROM dbo.foo
FOR XML RAW

SELECT *
    FROM dbo.foo
    FOR XML PATH(''), ROOT ('x'), TYPE

SET @x = (SELECT *
    FROM dbo.foo
    FOR XML PATH(''), ROOT ('x'), TYPE);

SELECT *
FROM dbo.foo
FOR JSON AUTO

SET @j = (
    SELECT *
    FROM dbo.foo
    FOR JSON PATH, INCLUDE_NULL_VALUES
);
