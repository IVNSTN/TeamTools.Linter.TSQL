begin transaction @tran_name;
save tran MY_SAVEPOINT;
rollback tran MY_SAVEPOINT;
commit tran MY_TRAN;

COMMIT
ROLLBACK
