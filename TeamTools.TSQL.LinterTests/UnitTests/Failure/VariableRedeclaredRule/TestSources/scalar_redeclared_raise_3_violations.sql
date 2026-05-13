CREATE PROC foo
    @a  INT
AS
BEGIN
    DECLARE @a CHAR         -- 1
        , @cr CURSOR

    EXEC bar
        @b,
        @value_a = @a

    DECLARE @cr CURSOR      -- 2
END
GO

DECLARE @foo INT = 1

PRINT @foo

DECLARE @foo INT -- 3
GO
