CREATE PROC dbo.test
    @id INT OUTPUT
AS
BEGIN
    EXEC dbo.foo
        @id = @id OUTPUT;
END
GO
DROP PROC dbo.test
GO
-- testing for fail tolerance
CREATE PROC dbo.no_param
AS
BEGIN
    SELECT 1
END
GO
CREATE PROC dbo.no_statements
AS;
