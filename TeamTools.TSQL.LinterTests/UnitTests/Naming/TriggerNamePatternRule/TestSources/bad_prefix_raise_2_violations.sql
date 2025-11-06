CREATE TRIGGER zoo.TRG_foo_ -- unexpected TRG prefix
ON zoo.foo
AFTER DELETE
AS
    RETURN;
GO

CREATE TRIGGER zoo.zoo_foo_del -- unexpected schema prefix
ON zoo.foo
AFTER DELETE
AS
    RETURN;
GO
