CREATE PROC dbo.foo
    @category_id INT
WITH RECOMPILE
AS
BEGIN
    SELECT *
    FROM dbo.bar
    WHERE category_id = @category_id
    OPTION (RECOMPILE) -- here
END
