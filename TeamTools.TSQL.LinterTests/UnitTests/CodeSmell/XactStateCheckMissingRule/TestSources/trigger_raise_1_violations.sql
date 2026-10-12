CREATE TRIGGER foo ON bar
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    INSERT foo (a, b)
    VALUES (1, 2)

    -- in a trigger XACT_ABORT is ON by default
    RAISERROR('adsf', 16, 1)

    -- tran is uncommittable after raiserror with XACT_ABORT enabled
    UPDATE bar SET
        lasdmod = GETDATE()
    WHERE id = @id
END
GO
