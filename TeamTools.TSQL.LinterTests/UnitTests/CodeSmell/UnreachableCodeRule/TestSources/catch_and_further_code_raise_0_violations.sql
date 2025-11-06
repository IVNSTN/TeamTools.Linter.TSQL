-- compatibility level min: 110
BEGIN TRY
    SELECT 1;

    THROW 50001, 'adsf', 1;
END TRY
BEGIN CATCH
    SELECT 3;
END CATCH;

SELECT 4;
