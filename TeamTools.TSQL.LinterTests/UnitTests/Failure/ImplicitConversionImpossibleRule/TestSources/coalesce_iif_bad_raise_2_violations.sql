-- compatibility level min: 110
DECLARE
    @dt     DATETIME
    , @int  INT
    , @time TIME
    , @guid UNIQUEIDENTIFIER;

SELECT
    COALESCE(@int, @time, '19000101')
    , IIF(1 = 0, @guid, @int);
