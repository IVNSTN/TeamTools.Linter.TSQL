CREATE TABLE #foo
(
    bar         INT   NOT NULL
    , yr        INT   CONSTRAINT df_yr DEFAULT IIF(0 < 1, 2, 3)
    , constrant ck_yr CHECK (0 > IIF(yr < 1, 2, 3)));

DECLARE @foo TABLE
(
    bar         INT   NOT NULL
    , yr        INT   DEFAULT IIF(0 < 1, 2, 3)
    , constrant ck_yr CHECK (0 > IIF(yr < 1, 2, 3)));
