SET XACT_ABORT ON

BEGIN TRAN

UPDATE foo SET upd = 1

-- failure but nothing afterwards - no violation
-- unpaired TRAN statement is not a concern of current rule
RAISERROR('test', 16, 1)
