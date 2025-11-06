BEGIN
    DECLARE @a INT;

    BEGIN
        SELECT 1;
        IF 1=0
        BEGIN
            RETURN 0;
        END
    END

    BEGIN
        SET @a = 'a';
        PRINT @a;
    END
END
