DECLARE
    @id           INT
    , @calc_value DECIMAL
    , @my_cursor  CURSOR;

SET @my_cursor = CURSOR FORWARD_ONLY FOR
    SELECT id, calc_value FROM dbo.foo
    FOR READ ONLY;

OPEN @my_cursor;

WHILE 1 = 1
BEGIN
    FETCH NEXT FROM @my_cursor
    INTO
        @id
        , @calc_value;

    IF @@FETCH_STATUS <> 0
        BREAK;

    UPDATE dbo.foo
    SET calc_value = 123
    WHERE CURRENT OF @my_cursor; -- here
END;

CLOSE @my_cursor;
DEALLOCATE @my_cursor;
