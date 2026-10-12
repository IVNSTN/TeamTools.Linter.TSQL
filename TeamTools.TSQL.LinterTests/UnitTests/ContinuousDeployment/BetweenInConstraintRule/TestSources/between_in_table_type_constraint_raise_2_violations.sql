CREATE TYPE foo AS TABLE
(
    bar   INT         NOT NULL CHECK (bar BETWEEN 1 AND 2)    -- 1
    , far VARCHAR(10) NOT NULL
    , mar DATETIME
    , CHECK (mar BETWEEN 1 AND 2) -- 2
);
