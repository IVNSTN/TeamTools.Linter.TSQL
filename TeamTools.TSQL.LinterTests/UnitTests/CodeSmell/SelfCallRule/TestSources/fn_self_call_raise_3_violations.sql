CREATE FUNCTION foo.bar (@id int)
RETURNS INT
AS
BEGIN
    SET @id = @id + 1;

    RETURN foo.[bar](@id);
END
GO

CREATE FUNCTION x.far (@id int)
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM x.far(@id)
)
GO

CREATE FUNCTION jar (@id int)
RETURNS @res TABLE
(
    title VARCHAR(100)
)
AS
BEGIN
    INSERT @res
    SELECT *
    FROM dbo.jar(@id)

    RETURN
END
GO
