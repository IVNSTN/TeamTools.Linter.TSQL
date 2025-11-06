DECLARE
    @id           INT
    , @calc_value DECIMAL;

DECLARE cr CURSOR FOR
    SELECT id, calc_value FROM dbo.foo
    FOR UPDATE;

OPEN cr;

WHILE 1 = 1
BEGIN
    FETCH NEXT FROM cr
    INTO
        @id
        , @calc_value;

    IF @@FETCH_STATUS <> 0
        BREAK;

    UPDATE dbo.foo
    SET calc_value = 123
    WHERE CURRENT OF cr;
END;

CLOSE cr;
DEALLOCATE cr;
