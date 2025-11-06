DECLARE @cr_rests CURSOR
    , @xml XML;

SET @cr_rests += 1 -- 1

SET @cr_rests = CURSOR LOCAL FORWARD_ONLY FOR
SELECT
    t.acc_code
    , t.currency
    , t.volume
FROM #transfers_from_selt AS t
WHERE t.err IS NULL
ORDER BY
    t.acc_code
    , t.currency
FOR UPDATE OF err;

OPEN @cr_rests;

FETCH NEXT FROM @cr_rests
INTO
    @acc_code
    , @currency
    , @volume;

WHILE @@FETCH_STATUS = @xml -- 2
BEGIN
    FETCH NEXT FROM @cr_rests
    INTO
        @acc_code
        , @currency
        , @volume;
END;

CLOSE @cr_rests;
DEALLOCATE @cr_rests;
