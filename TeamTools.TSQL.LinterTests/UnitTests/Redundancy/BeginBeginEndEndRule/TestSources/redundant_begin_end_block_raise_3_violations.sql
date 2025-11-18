BEGIN
    BEGIN -- 1
        IF 1=0
        BEGIN
            RETURN 0;
        END
    END

    BEGIN -- 2
        BEGIN -- 3
            DECLARE @a INT;
            PRINT 'a'
        END
    END
END
