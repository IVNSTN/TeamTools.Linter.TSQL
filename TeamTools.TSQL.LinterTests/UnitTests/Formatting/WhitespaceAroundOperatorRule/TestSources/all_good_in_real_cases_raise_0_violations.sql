-- compatibility level min: 110
SELECT @b -= IIF( 3 <= 4, 0, -1);

SET @Body =
    'Information message only.' + CHAR(13) + 'сбой отправки сообщения в цикле ' + CAST((@Counter - 1) AS VARCHAR(10)) + CHAR(13)
    + ISNULL(@ErrMess, '*') + CHAR(13) + 'Создан новый conversation handler.';

IF @Counter > 10
    OR XACT_STATE() = -1
BEGIN
    RAISERROR(N'Failed to SEND: %s', 16, 3, @ErrMess);

    BREAK;
END;

SET @v = 
    CASE WHEN - (3) = 0
        THEN -1
        ELSE -2
    END;

IF XACT_STATE() IN (-1, 1)
BEGIN
    ROLLBACK TRAN;
END;
