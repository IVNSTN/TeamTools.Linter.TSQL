CREATE TYPE foo AS TABLE
(
    id      INT
        , name  VARCHAR -- 1
    , dt    DATE
)

CREATE TABLE #bar
(
    id      INT
    , name  VARCHAR
    , dt  DATE          -- 2
)

CREATE TABLE #bar
(
    id      INT
    , name  VARCHAR
    , calc    AS 1 + 1  -- 3
)
