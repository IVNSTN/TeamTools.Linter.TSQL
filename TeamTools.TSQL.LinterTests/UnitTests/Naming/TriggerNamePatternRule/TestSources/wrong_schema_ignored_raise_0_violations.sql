-- mismatching trigger schema is controlled by a separate rule
CREATE TRIGGER xxx.foo_instead_upd
ON zoo.foo
INSTEAD OF INSERT, UPDATE
AS
    RETURN;
GO
