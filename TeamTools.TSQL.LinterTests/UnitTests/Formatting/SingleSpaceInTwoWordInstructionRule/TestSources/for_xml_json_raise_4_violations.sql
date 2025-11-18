-- compatibility level min: 130
SELECT *
FROM dbo.foo
FOR XML   RAW                               -- 1


SET @x = (SELECT *
    FROM dbo.foo
    FOR XML
      PATH(''), ROOT ('x'), TYPE);          -- 2

SELECT *
FROM dbo.foo
FOR    JSON AUTO                            -- 3

SET @j = (
    SELECT *
    FROM dbo.foo
    FOR JSON   PATH, INCLUDE_NULL_VALUES    -- 4
);
