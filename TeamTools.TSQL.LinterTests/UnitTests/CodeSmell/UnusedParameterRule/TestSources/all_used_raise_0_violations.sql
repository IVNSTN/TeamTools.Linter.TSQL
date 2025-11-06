CREATE PROCEDURE dbo.foo
    @used  INT = NULL
    , @tvp dbo.my_tvp READONLY
AS
BEGIN
    SELECT * FROM @tvp WHERE id = @used;
END;
