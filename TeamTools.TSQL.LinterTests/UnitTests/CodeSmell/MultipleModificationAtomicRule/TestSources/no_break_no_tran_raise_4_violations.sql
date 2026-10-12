INSERT foo (a, b)
VALUES (1, 2)

UPDATE bar SET
    lasdmod = GETDATE()
WHERE id = @id

GO
SET XACT_ABORT OFF

DELETE t
FROM tar

MERGE trg
USING src
ON src.id = trg.id
WHEN MATCHED THEN
    DELETE;
GO
