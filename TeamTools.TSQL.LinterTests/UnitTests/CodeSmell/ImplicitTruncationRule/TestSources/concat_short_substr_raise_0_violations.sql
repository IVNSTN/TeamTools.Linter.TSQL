DECLARE
    @res_code        CHAR(1)      = 'N'
    , @summary       VARCHAR(255) = 'Предача в архив.'
    , @row_count     INT = @@ROWCOUNT

    SET @summary += ' Строк для передачи: ' + CAST(@row_count AS VARCHAR(10));
