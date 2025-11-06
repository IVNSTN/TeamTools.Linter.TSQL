CREATE TRIGGER zoo.foo_upd -- missing instead_ suffix
ON zoo.foo
INSTEAD OF INSERT, UPDATE
AS
    RETURN;
GO

CREATE TRIGGER zoo.foo -- missing description suffix
ON zoo.foo
AFTER DELETE
AS
    RETURN;
GO
