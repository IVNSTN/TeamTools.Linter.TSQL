DECLARE
    @name   VARCHAR(10) = 'name'
    , @num  INT
    , @date DATE;

IF @num > 1                             -- 1
    SELECT t.id, @name AS foo_name
    FROM dbo.foo AS t
    WHERE t.date < @date;               -- 2
GO

CREATE PROCEDURE dbo.bar
AS
BEGIN
    DECLARE
        @name   VARCHAR(10) = 'name'
        , @num  INT
        , @date DATE;

    IF @num > 1                         -- 3
        SELECT t.id, @name AS foo_name
        FROM dbo.foo AS t
        WHERE t.date < @date;           -- 4

    DECLARE @n NUMERIC(10,2)

    EXEC dbo.foobar
        @n = @n                         -- 5
END;
