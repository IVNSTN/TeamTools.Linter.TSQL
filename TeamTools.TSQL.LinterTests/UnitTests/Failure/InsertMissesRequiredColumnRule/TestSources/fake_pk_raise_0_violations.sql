CREATE TABLE #trades
(
    trd_no   INT            NOT NULL
    , volume NUMERIC(17, 2) NULL PRIMARY KEY CLUSTERED (trd_no ASC) -- volume is not a real PK
);

INSERT #trades (trd_no)
VALUES (1);
