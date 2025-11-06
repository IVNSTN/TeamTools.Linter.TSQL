CREATE TYPE foo AS TABLE
(
    bar         INT   NOT NULL
    , yr        INT   NOT NULL DEFAULT IIF(yr < 1, 2, 3)         -- 1
    , CHECK (0 > IIF(yr < 1, 2, 3)));   -- 2
