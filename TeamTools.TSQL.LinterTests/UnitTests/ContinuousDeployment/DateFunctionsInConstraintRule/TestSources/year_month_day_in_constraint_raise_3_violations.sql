CREATE TABLE foo
(
    bar         INT   NOT NULL
    , yr        INT   CONSTRAINT df_yr DEFAULT YEAR(GETDATE())  -- 1 - YEAR
    , constrant ck_yr CHECK (yr > MONTH(GETDATE())));           -- 2 - MONTH

ALTER TABLE foo
ADD constrant ck_yr2 CHECK (yr < DAY(GETDATE()))                -- 3 - DAY
