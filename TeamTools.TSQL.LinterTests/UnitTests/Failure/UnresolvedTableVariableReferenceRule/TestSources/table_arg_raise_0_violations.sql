CREATE PROC foo
    @src some_table_type READONLY
AS
BEGIN
    SELECT *
    FROM @src
END
GO
