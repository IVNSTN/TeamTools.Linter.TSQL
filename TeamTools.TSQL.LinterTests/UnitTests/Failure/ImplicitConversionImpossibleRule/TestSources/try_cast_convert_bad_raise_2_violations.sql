-- compatibility level min: 110
DECLARE
    @int  INT
    , @dt DATE;

SELECT
    @dt = TRY_CAST('0' AS TIME)
    , @dt = TRY_CONVERT(TIME, '0');
