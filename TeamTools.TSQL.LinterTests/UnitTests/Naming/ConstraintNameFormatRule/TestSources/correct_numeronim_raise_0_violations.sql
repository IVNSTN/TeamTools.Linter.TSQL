CREATE TABLE xxx.test
(
    a   INT NOT NULL
    , b INT
    , c INT
    , d INT
    , e INT
    , g INT
    , CONSTRAINT UQ_xxx_test_a_b7g UNIQUE (a, b, c, d, e, g) -- b> _c_d_e_ <g
);
GO

CREATE TABLE aaa.test
(
    abcdef   INT NOT NULL
    , ghijkl INT
    , mkopqr DATETIME
    , CONSTRAINT UQ_aaa_test_abcdef_m11l UNIQUE (abcdef, mkopqr, ghijkl)
);
