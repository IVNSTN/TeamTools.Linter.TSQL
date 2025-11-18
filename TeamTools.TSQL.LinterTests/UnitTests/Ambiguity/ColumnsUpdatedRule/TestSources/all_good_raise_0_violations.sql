CREATE TRIGGER dml ON dbo.foo
AFTER INSERT
AS
BEGIN
    IF UPDATE(my_col)
    BEGIN
        RAISERROR ('asdf', 16, 1);
    END
END
