    BEGIN TRY
        SELECT 2
        IF 1 = 0
BEGIN 
            SELECT 3
        END
    END TRY
    BEGIN CATCH
        SELECT 6
        IF 0 = 1
        BEGIN
            SELECT 7
END
    END CATCH
