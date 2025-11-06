CREATE TRIGGER zoo.foo_instead_upd
ON zoo.foo
INSTEAD OF INSERT, UPDATE
AS
    RETURN;
GO

CREATE TRIGGER zoo.foo_rollback_when_no_access
ON zoo.foo
AFTER DELETE
AS
    RETURN;
GO
