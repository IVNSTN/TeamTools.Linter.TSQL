CREATE FUNCTION dbo.my_fn ()
RETURNS INT
AS
BEGIN
    DECLARE @var INTEGER    -- integer is an INT alias
    RETURN @var
END;
GO
