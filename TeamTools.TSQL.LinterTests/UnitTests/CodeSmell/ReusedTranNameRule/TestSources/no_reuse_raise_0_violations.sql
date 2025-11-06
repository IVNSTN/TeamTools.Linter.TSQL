BEGIN TRAN;

INSERT dbo.foo (id) VALUES (1);

COMMIT TRAN;

BEGIN TRAN my_tran;

UPDATE dbo.foo SET title = 'upd' WHERE id = 1;

SAVE TRAN my_savepoint;

DELETE dbo.bar WHERE id = 1;

COMMIT TRAN my_savepoint;

ROLLBACK TRAN my_tran;
