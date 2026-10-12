CREATE PROCEDURE dbo.bar
    @x   INT
    , @y INT
AS
BEGIN
    SELECT @x += 1
    FROM foo

    UPDATE far SET
        @y += 2
    FROM far
END
