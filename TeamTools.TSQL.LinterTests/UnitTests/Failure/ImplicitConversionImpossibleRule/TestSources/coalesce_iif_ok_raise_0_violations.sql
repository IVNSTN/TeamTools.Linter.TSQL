-- compatibility level min: 110
DECLARE
    @dt     DATETIME
    , @int  INT
    , @time TIME
    , @guid UNIQUEIDENTIFIER;

SELECT
    COALESCE(@int, @dt, '19000101')
    , IIF(1 = 0, @guid, '111-11-11');
