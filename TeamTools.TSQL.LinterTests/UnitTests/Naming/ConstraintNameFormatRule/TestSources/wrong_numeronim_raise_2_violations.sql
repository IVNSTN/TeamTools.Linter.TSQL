CREATE TABLE xxx.test
(
    a   INT NOT NULL
    , b INT
    , c INT
    , d INT
    , e INT
    , g INT
    , CONSTRAINT UQ_xxx_test_a_b07g UNIQUE (a, b, c, d, e, g) -- unexpected 0 prefix
);
GO

CREATE TABLE aaa.test
(
    abcdef   INT NOT NULL
    , ghijkl INT
    , mkopqr DATETIME
    , CONSTRAINT UQ_aaa_test_abcdef_m10101l UNIQUE (abcdef, mkopqr, ghijkl)
);
