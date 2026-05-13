CREATE PROC foo
    @a  INT
AS
BEGIN
    DECLARE @a TABLE (id INT)           -- 1

    DECLARE @t TABLE (title VARCHAR)

    DECLARE @t TABLE (dt DATE)          -- 2

    DECLARE @t TIME                     -- 3
END
