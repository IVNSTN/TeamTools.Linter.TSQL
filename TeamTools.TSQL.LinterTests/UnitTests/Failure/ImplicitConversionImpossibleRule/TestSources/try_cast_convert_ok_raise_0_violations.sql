-- compatibility level min: 110
DECLARE
    @int  INT
    , @dt DATETIME;

SELECT
    @int = TRY_CAST('0' AS INT)
    , @int = TRY_CONVERT(BIT, '0')
