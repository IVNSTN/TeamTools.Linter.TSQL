CREATE TABLE foo
(
    bar   INT         NOT NULL DEFAULT 1
    , far VARCHAR(10) NOT NULL CONSTRAINT ck CHECK (CONVERT(VARCHAR(3), far) = 'zar' OR far = 'jar')
    , mar DATETIME    CONSTRAINT df_mar DEFAULT CONVERT(DATE, GETDATE())
    , bar AS ((1 + CONVERT(INT, '1')))
    , CONSTRAINT pk_foo PRIMARY KEY CLUSTERED (bar)
);
