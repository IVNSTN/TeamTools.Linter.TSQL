BEGIN
    SELECT 1
    BEGIN TRY
        SELECT 2
        IF 1 = 0
        BEGIN
            SELECT 3
        END
        ELSE
        BEGIN
            SELECT 4
            BEGIN
                SELECT 5
            END
        END
    END TRY
    BEGIN CATCH
        SELECT 6
        IF 0 = 1
        BEGIN
            SELECT 7
        END
    END CATCH
END

BEGIN
    BEGIN TRY
        BEGIN
            SELECT 1
        END
    END TRY
    BEGIN CATCH
        BEGIN
            SELECT 2
        END
    END CATCH
END
