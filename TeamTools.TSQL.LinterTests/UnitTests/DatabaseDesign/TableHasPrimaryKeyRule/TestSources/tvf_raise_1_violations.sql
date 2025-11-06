CREATE FUNCTION dbo.my_tvf (@id INT)
RETURNS @res TABLE (id INT NULL) -- here
AS
BEGIN
    SELECT * FROM @res
    WHERE id = @id
END;
GO
