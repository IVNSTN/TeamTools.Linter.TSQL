CREATE TRIGGER zoo.foo_t -- suffix is shorter than 3 symbols
ON zoo.foo
AFTER DELETE
AS
    RETURN;
GO
