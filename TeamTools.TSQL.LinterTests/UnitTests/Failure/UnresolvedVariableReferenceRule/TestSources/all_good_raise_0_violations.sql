-- inside proc with respect to args
CREATE PROC foo
    @a  INT
AS
BEGIN
    DECLARE @b CHAR, @cr CURSOR

    EXEC bar
        @b,
        @value_a = @a

    SELECT @a
    WHERE @b > 0

    OPEN @cr
END
GO

-- in script root
DECLARE @foo INT = 1

PRINT @foo
GO
