CREATE PROCEDURE dbo.test2
    @param1   INT NULL
    , @param2 BIT
AS
;
GO

CREATE FUNCTION dbo.test3 (@param1 INT NULL, @param2 BIT)
RETURNS INT
AS
BEGIN
    RETURN 1;
END;
GO
