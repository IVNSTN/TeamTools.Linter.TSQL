CREATE PROC dbo.foo
AS
BEGIN
    CREATE TABLE #test (name varchar(100))

    DELETE #test
END
GO
