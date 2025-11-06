CREATE PROCEDURE dbo.bar
AS
BEGIN TRY
    SET NOCOUNT ON;

    RETURN 0;
END TRY
BEGIN CATCH
    RETURN 1;
END CATCH
GO

CREATE TRIGGER dbo.bar on dbo.jar after delete
as
begin try
    rollback
end try
begin catch
    commit
end catch;
GO
