CREATE TABLE foo
(
    bar         INT   NOT NULL
    , yr        INT   CONSTRAINT df_yr DEFAULT IIF(yr < 1, 2, 3)    -- 1
    , constrant ck_yr CHECK (0 > IIF(yr < 1, 2, 3)));               -- 2

ALTER TABLE foo
ADD constraint df_bar  DEFAULT IIF(0=0, 1, 2) for bar;              -- 3
