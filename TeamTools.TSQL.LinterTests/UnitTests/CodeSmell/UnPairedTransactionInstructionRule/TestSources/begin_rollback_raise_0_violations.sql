BEGIN TRAN trn

DELETE dbo.foo WHERE bar = 1

IF @@TRANCOUNT > 0
    ROLLBACK TRAN trn

-- and withoud names
BEGIN TRAN

DELETE dbo.foo WHERE bar = 1

IF @@TRANCOUNT > 0
    ROLLBACK TRAN
