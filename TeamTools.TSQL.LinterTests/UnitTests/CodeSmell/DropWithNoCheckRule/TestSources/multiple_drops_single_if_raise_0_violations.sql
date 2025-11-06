IF ((OBJECT_ID('dbo.foo', 'U') IS NOT NULL)
AND NOT OBJECT_ID('dbo.bar', 'U') IS NULL)
AND OBJECT_ID('#far') IS NOT NULL
BEGIN
    BEGIN
        DROP TABLE dbo.foo;
        DROP TABLE bar;
    END;

    DROP TABLE #far;
END;
GO
