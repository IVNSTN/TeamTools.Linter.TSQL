SET XACT_ABORT ON

BEGIN TRAN

UPDATE foo SET upd = 1

RAISERROR('test', 16, 1)

-- RAISERROR did make tran uncommittable
INSERT bar
VALUES (1, 2)

COMMIT TRAN
