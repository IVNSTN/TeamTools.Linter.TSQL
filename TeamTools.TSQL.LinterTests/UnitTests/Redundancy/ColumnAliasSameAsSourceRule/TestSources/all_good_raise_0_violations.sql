SELECT
    src.foo + 1 AS foo,
    src.bar AS far,
    src.jar
FROM dbo.tar

GO

MERGE trg
USING src
ON trg.id = src.id
WHEN MATCHED THEN
    UPDATE SET dt = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (dt) VALUES (GETDATE())
OUTPUT $action, $action AS act, src.id, INSERTED.id;
