DECLARE
    @a    INT,
    @name   VARCHAR(100)
    , @x  BIT
GO
CREATE PROC bar
    @foo  INT
    , @zoo  MONEY
AS
SELECT 1
GO
CREATE FUNCTION zar (
    @foo        INT,
    @zoo    DECIMAL(12,2)
) RETURNS INT
AS
BEGIN
RETURN 1
END;
