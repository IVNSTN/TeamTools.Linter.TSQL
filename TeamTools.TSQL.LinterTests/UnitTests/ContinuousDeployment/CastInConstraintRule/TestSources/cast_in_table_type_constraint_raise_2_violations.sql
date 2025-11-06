CREATE TYPE foo AS TABLE
(
    bar   INT         NOT NULL DEFAULT CAST('test' AS INT)   -- 1
    , far VARCHAR(10) NOT NULL
    , mar DATETIME
    , CHECK (CAST(far AS VARCHAR(3)) = 'zar' OR far = 'jar') -- 2
);
