CREATE TABLE foo
(
    bar   INT         NOT NULL DEFAULT 1
    , far VARCHAR(10) NOT NULL CONSTRAINT ck CHECK (far >= 'a' AND far <= 'z')
    , mar DATETIME    CONSTRAINT df_mar DEFAULT CONVERT(DATE, GETDATE())
    , bar AS ((1 + CONVERT(INT, '1')))
    , CONSTRAINT pk_foo PRIMARY KEY CLUSTERED (bar)
);
