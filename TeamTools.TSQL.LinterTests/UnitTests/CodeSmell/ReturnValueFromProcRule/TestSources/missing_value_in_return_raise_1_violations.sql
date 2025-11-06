CREATE PROCEDURE dbo.foo
AS
BEGIN
    -- no return;
    SELECT 0;
    RETURN;
END
