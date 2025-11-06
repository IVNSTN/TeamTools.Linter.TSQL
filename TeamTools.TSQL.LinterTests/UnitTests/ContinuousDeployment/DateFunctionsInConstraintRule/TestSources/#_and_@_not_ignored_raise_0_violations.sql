CREATE TABLE #foo
(
    bar         INT   NOT NULL
    , yr        INT   CONSTRAINT df_yr DEFAULT YEAR(GETDATE())
    , constrant ck_yr CHECK (yr > MONTH(GETDATE()))
    , constrant ck_yr2 CHECK (yr < DAY(GETDATE())));

DECLARE @foo TABLE
(
    bar         INT   NOT NULL
    , yr        INT   DEFAULT YEAR(GETDATE())
    , constrant ck_yr CHECK (yr > MONTH(GETDATE()))
    , constrant ck_yr2 CHECK (yr < DAY(GETDATE())));
