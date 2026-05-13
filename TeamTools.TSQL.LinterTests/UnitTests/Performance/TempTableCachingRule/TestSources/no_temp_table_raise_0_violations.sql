CREATE PROC foo
    @client_id INT
WITH RECOMPILE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.bar
    WHERE client_id = @client_id

    ALTER TABLE dbo.far
    DROP CONSTRAINT CS

    RETURN 1
END
GO
