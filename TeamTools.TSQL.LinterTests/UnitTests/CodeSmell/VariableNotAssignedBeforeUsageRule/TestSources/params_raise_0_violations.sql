CREATE PROCEDURE dbo.foo
    @id INT,
    @rows dbo.bar READONLY
AS
BEGIN
    SELECT title
    FROM dbo.foo f
    WHERE id = @id
        AND NOT EXISTS(SELECT 1 FROM @rows r WHERE r.id = f.id);
END;
GO

DECLARE @id INT, @name VARCHAR(100)

EXEC dbo.foo
    @id = 1
    , @name = 'adsf';
