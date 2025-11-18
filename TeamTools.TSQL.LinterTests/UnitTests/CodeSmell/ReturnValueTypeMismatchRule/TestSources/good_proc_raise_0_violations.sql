CREATE PROC dbo.zero_return
AS
BEGIN
    RETURN 0
END
GO

CREATE PROC dbo.null_return
AS
BEGIN
    -- pointless return value is not analyzed here
    RETURN NULL
END
GO

CREATE PROC dbo.var_return
    @arg INT
AS
BEGIN
    DECLARE @another_int SMALLINT

    RETURN @arg

    -- unreachable code is not analyzed in this rule
    RETURN @another_int
    RETURN ISNULL(@another_int, 0)
END
GO

CREATE PROC dbo.no_return
AS
    SELECT NULL AS [RETURN]
GO

CREATE PROC dbo.calc_return
AS
BEGIN
    SET NOCOUNT ON;

    RETURN 1 + 1;
END;
GO
