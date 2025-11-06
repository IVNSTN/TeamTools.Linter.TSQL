DECLARE cr CURSOR FAST_FORWARD FOR
    SELECT id, title FROM dbo.foo;

OPEN cr;

FETCH NEXT FROM cr
INTO
    @id
    , @title;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @id += 1; -- somewhat usage

    PRINT @title; -- title is used

    FETCH NEXT FROM cr
    INTO
        @id
        , @title;
END;

CLOSE cr;
DEALLOCATE cr;
