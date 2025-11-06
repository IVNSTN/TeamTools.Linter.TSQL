-- compatibility level min: 110
SELECT
    try_cast(try_convert(NUMERIC(10,2), '0') AS NUMERIC(10,2))
    , iif (0=0,1,1)
