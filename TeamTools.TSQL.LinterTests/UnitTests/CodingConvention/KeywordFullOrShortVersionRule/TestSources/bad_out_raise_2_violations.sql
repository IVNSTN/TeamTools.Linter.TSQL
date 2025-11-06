CREATE PROCEDURE dbo.test
    @id INT OUT
AS
BEGIN
    EXEC dbo.foo
        @id = @id OUT;
END
