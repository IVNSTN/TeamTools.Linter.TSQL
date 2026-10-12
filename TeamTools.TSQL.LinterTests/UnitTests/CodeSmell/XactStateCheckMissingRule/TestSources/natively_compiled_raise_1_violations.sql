-- compatibility level min: 130
CREATE PROC my_proc
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
    INSERT foo (a, b)
    VALUES (1, 2)

    IF 1=0
        RAISERROR('adsf', 16, 1)

    -- XACT_ABORT is on in natively compiled proc
    -- tran is uncommittable after error
    UPDATE bar SET
        lasdmod = GETDATE()
    WHERE id = @id
END
GO
