CREATE INDEX ix ON test (a)
GO
ALTER TABLE foo add x int;
GO
DROP TABLE jar;
GO
CREATE STATISTICS stats on bar (x)
GO
CREATE TRIGGER io ON zar after insert as rollback;
