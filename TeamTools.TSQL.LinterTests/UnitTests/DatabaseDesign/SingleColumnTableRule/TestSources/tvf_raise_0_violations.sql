CREATE FUNCTION foo (@arg INT)
RETURNS @res TABLE
(
    some_id INT
)
AS
BEGIN
    INSERT @res(id)
    VALUES (1)

    RETURN
END
