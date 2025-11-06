CREATE PROC dbo.foo
AS
BEGIN
    SELECT 'a' AS a
    RETURN 1
END
-- here
GRANT EXEC ON dbo.foo TO PUBLIC AS dbo;
