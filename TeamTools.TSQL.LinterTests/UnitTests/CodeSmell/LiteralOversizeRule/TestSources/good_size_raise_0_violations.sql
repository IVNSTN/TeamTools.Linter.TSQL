DECLARE @str VARCHAR(10)

SET @str = 'asdf' + '1'

SET @str = ISNULL(@str, '1234567890')
SELECT ISNULL(@str, '')
GO

DECLARE @name VARCHAR(5) = 'ABCDE'
GO
DECLARE
    @RetCode    INT
    , @ErrorMsg VARCHAR(2000);

EXEC @RetCode = foo.bar
    @error_msg = @ErrorMsg OUTPUT;

IF @RetCode <> 0
BEGIN
    SET @ErrorMsg = '. ' + ISNULL(@ErrorMsg, 'foo.bar failed');
END;
GO
