CREATE TABLE a (
    b INT NOT NULL DEFAULT CASE WHEN 'x' = 'a' or 'x' = 'b' or 'x' = 'c' THEN 1 ELSE 0 END
    , c CHAR(1) NULL CHECK (c = 'B' or c= 'S'));
