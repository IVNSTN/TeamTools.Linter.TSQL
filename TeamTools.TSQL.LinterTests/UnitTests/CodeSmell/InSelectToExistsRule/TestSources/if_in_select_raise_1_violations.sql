IF @id IN (SELECT Id FROM dbo.foo)
    RETURN 1
