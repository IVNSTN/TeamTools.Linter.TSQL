CREATE PROCEDURE foo.bar
    @arg VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @err_msg VARCHAR(4000) = 'Ⓒ'; -- there is a separate rule to detect hardcoded broken literals

    SET @err_msg += 'adsf';

    RAISERROR(@err_msg, 16, 1);
END;
GO
