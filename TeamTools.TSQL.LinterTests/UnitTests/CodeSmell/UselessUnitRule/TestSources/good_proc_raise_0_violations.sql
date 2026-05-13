CREATE PROC dbo.foo
    @arg INT
AS
BEGIN
    SET NOCOUNT ON

    CREATE TABLE #t (id INT)

    INSERT #t(id)
    SELECT id
    FROM dbo.bar
    WHERE category_id = @arg

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR ('Bad key', 16, 1)
        RETURN 1
    END

    RETURN 0
END
