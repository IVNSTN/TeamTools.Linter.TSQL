CREATE TABLE #a (
    c CHAR(1) NULL CHECK (c in ('B', 'S')));

ALTER TABLE #a ADD CONSTRAINT ck_test CHECK (c in ('B', 'S'));

DECLARE @a TABLE (
    c CHAR(1) NULL CHECK (c in ('B', 'S')));
