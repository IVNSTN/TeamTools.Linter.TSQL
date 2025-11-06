DECLARE
    @id           INT
    , @calc_value DECIMAL;

DECLARE cr CURSOR FOR
    -- no FOR UPDATE
    SELECT id, calc_value FROM dbo.foo;

OPEN cr;

WHILE 1 = 1
BEGIN
    FETCH NEXT FROM cr
    INTO
        @id
        , @calc_value;

    IF @@FETCH_STATUS <> 0
        BREAK;

    -- no WHERE CURRENT OF
    UPDATE opt
    SET calc_value = @calc_value
    FROM #operations AS opt
    INNER JOIN dbo.OperationTypes AS ot
        ON opt.IdOperationType = ot.IdOperationType;

    PRINT @calc_value
END;

CLOSE cr;
DEALLOCATE cr;
