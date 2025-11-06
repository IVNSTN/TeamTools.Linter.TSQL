BEGIN TRAN FirstTran

SAVE TRAN [my important tran]

COMMIT TRAN my_tran

ROLLBACK TRANSACTION @my_tran
