
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH
    -- nothing
END CATCH

-- table variables do not participate in user-defined transaction
INSERT @foo (a, b)
VALUES (1, 2)

UPDATE @bar SET
    lasdmod = GETDATE()
WHERE id = @id
GO
