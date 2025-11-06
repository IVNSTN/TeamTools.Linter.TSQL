-- compatibility level min: 110
CREATE TABLE a (
    b INT NOT NULL DEFAULT IIF('x' IN ('a', 'b', 'c'), 1,0)
    , c CHAR(1) NULL CHECK (IIF(c in ('B', 'S'), 0, 1) = 1));
