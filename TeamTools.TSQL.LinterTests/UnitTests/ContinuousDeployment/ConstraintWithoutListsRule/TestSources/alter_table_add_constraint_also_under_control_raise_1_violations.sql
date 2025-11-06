CREATE TABLE a (
    b INT NOT NULL
    , c CHAR(1) NULL);

ALTER TABLE a ADD CONSTRAINT ck_check CHECK (c in ('B', 'S'))
