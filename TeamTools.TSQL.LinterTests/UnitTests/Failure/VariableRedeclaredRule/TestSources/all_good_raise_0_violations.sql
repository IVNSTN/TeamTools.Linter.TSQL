CREATE PROC foo
    @a  INT
AS
BEGIN
    DECLARE @b CHAR, @cr CURSOR

    EXEC bar
        @b,
        @value_a = @a

    SELECT @a, @b

    DECLARE @tbl TABLE (id INT)

    CREATE TABLE #tbl (id INT)

    OPEN @cr
END
GO
-- in script root
DECLARE @foo INT = 1

PRINT @foo

DECLARE @tbl TABLE (id INT)
GO
