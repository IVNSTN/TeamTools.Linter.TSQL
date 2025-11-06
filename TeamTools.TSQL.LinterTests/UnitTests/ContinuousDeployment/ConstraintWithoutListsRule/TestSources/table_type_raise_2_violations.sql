CREATE TYPE a AS TABLE (
    b INT NOT NULL DEFAULT CASE WHEN (('x' IN ('a', 'b', 'c'))) THEN 1 END
    , c CHAR(1) NULL CHECK (c in ('B', 'S')));
