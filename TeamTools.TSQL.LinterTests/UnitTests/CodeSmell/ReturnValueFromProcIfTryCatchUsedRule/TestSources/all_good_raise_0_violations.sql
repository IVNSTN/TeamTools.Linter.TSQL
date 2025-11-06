CREATE PROCEDURE dbo.foo
AS
BEGIN
    -- no try-catch, no return
    SELECT 1;
END
GO
ALTER PROCEDURE dbo.foo
AS
BEGIN
    -- no try-catch but return
    RETURN 1;
END
GO
ALTER PROCEDURE dbo.foo
AS
BEGIN
    BEGIN TRY
        SELECT 1
        RETURN 0;
    END TRY
    BEGIN CATCH
        -- return in the catch
        RETURN 1;
    END CATCH
END
GO
ALTER PROCEDURE dbo.foo
AS
BEGIN
    BEGIN TRY
        SELECT 1
        RETURN 0;
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE()
    END CATCH

    -- return after catch
    RETURN 0;
END
