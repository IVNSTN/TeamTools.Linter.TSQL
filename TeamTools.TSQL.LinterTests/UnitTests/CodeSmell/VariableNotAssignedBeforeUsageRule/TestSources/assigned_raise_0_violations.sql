DECLARE
    @name   VARCHAR(10) = 'name'
    , @num  INT
    , @date DATE;

SELECT @num = t.num FROM dbo.foo;

SET @date = GETDATE();

IF @num > 1
    SELECT t.id, @name AS foo_name
    FROM dbo.foo AS t
    WHERE t.date < @date;
GO

CREATE PROCEDURE dbo.bar
AS
BEGIN
    DECLARE
        @name   VARCHAR(10) = 'name'
        , @num  INT
        , @date DATE;

    SELECT @num = t.num FROM dbo.foo;

    SET @date = GETDATE();

    IF @num > 1
        SELECT t.id, @name AS foo_name
        FROM dbo.foo AS t
        WHERE t.date < @date;

    DECLARE @n NUMERIC(10,2)

    EXEC dbo.foobar
        @n = @n OUTPUT

    SELECT @n AS n
END;
