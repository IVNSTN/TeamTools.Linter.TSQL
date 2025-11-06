CREATE FUNCTION dbo.foo(@id INT)
RETURNS @result TABLE
(
    id INT,
    title VARCHAR
)
AS
BEGIN
    INSERT @result
    VALUES (1, '')

    RETURN
END
