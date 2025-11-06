DECLARE cr CURSOR FAST_FORWARD FOR
    SELECT id, title FROM dbo.foo;

OPEN cr;

FETCH NEXT FROM cr
INTO
    @id -- only first occurance is detected in current implementation
    , @title;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- @id never used
    PRINT @title; -- title is used

    FETCH NEXT FROM cr
    INTO
        @id
        , @title;

    FETCH LAST FROM cr
    INTO
        @id
        , @title;
END;

FETCH FIRST FROM cr
INTO
    @id
    , @title;

CLOSE cr;
DEALLOCATE cr;
