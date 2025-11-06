CREATE PROCEDURE dbo.run_cursor
    @debug BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @my_cursor   CURSOR
        , @type_code VARCHAR(10)
        , @row_id    INT
        , @other_id  BIGINT;

    SET @my_cursor = CURSOR LOCAL STATIC FORWARD_ONLY READ_ONLY FOR
    -- there is a separate rule for matching selected col count and fetched var count
    SELECT tbd;

    OPEN @my_cursor;

    FETCH NEXT FROM @my_cursor
    INTO
        @type_code
        , @row_id
        , @other_id;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @counter += 1;

        IF @debug = 1
        BEGIN
            SELECT @type_code AS [@type_code]
                , @row_id AS [@row_id]
                , @other_id AS [@other_id];
        END

        FETCH NEXT FROM @my_cursor
        INTO
            @type_code
            , @row_id
            , @other_id;
    END;

    CLOSE @my_cursor;
    DEALLOCATE @my_cursor;
END;
