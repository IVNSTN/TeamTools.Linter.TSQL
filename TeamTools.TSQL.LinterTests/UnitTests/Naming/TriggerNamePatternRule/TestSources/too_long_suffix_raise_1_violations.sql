CREATE TRIGGER zoo.foo_rollback_when_no_access_validate_each_row -- longer 24 symbols
ON zoo.foo
AFTER DELETE
AS
    RETURN;
GO
