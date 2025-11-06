CREATE PROCEDURE dbo.test
    @id INT OUTPUT
AS
BEGIN
    EXECUTE dbo.foo
        @id = @id OUTPUT;
END
