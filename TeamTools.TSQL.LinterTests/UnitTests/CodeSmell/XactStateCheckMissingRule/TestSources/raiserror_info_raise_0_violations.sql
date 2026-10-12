BEGIN TRAN

UPDATE foo SET upd = 1

RAISERROR('test', 0, 0)

-- RAISERROR did not affect tran
INSERT bar
VALUES (1, 2)

COMMIT TRAN
GO

SET XACT_ABORT OFF

BEGIN TRAN

UPDATE foo SET upd = 1

RAISERROR('test', 16, 1)

-- RAISERROR did NOT make tran uncommittable
INSERT bar
VALUES (1, 2)

COMMIT TRAN
