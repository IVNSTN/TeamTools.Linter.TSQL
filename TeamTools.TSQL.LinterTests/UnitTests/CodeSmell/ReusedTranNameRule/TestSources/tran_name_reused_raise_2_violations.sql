BEGIN TRAN my_tran;

UPDATE dbo.foo SET title = 'upd' WHERE id = 1;

SAVE TRAN my_tran; -- 1

DELETE dbo.bar WHERE id = 1;

COMMIT TRAN my_tran;

BEGIN TRAN my_tran; -- 2

DELETE dbo.bar WHERE id = 1;

COMMIT TRAN my_tran;
