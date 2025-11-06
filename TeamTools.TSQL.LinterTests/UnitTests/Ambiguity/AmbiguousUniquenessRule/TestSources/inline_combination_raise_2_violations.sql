DECLARE @t TABLE
(
    id      INT PRIMARY KEY
    , title VARCHAR(30)
    , dt    DATETIME
    , UNIQUE (title)
    , UNIQUE (title, id, dt) -- 1, 2 - id and title are already unique
);
