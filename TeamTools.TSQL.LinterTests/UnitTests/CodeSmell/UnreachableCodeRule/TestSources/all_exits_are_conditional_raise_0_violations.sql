-- compatibility level min: 110
BEGIN TRY
    IF @ret_code >= 50000
        THROW @ret_code, @ret_msg, 1;
    ELSE IF @ret_code > 0
        THROW 50000, @ret_msg, 1;

    RETURN 0;
END TRY
BEGIN CATCH
    SELECT 1;
END CATCH;
