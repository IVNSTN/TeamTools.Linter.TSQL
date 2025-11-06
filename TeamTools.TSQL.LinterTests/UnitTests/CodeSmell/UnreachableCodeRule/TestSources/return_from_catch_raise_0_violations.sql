BEGIN TRY
    SELECT 1
    RETURN 1
END TRY
BEGIN CATCH
    SELECT 3
END CATCH
-- still 'reachable' because TRY may fall to CATCH and never reach return
SELECT 4
