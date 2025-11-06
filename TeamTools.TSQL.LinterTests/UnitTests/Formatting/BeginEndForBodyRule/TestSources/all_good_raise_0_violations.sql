CREATE PROCEDURE dbo.foo
    @bar INT = NULL OUTPUT
WITH EXECUTE AS OWNER
AS
/* comment
*/    -- sadf
BEGIN
    select 1
    if 'a' = 'b'
    BEGIN
        exec dbo.my_proc;
    END;
    RETURN 1
END;
GO

CREATE TRIGGER zar
ON dbo.acme
AFTER INSERT AS
BEGIN
    ROLLBACK TRAN
    RETURN;
END
GO

CREATE FUNCTION dbo.tf()
RETURNS @t TABLE (id int)
AS
BEGIN
    INSERT @t(id)
    VALUES (1)

    RETURN;
END
GO

CREATE FUNCTION dbo.fn()
RETURNS INT
AS
BEGIN
    RETURN 42;
END
GO

CREATE FUNCTION dbo.i_f()
RETURNS TABLE
AS
    RETURN
        SELECT 1 as id;
