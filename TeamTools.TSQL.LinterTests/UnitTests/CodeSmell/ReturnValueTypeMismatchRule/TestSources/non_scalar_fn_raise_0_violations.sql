CREATE FUNCTION dbo.my_split_fn (@parent_id INT)
RETURNS TABLE
AS
    RETURN
    SELECT id FROM dbo.foo_bar;
GO

CREATE FUNCTION dbo.my_split_fn (@parent_id INT)
RETURNS @tbl TABLE (row_id INT)
AS
BEGIN
    INSERT @tbl
    SELECT id FROM dbo.foo_bar;

    RETURN;
END;
GO
