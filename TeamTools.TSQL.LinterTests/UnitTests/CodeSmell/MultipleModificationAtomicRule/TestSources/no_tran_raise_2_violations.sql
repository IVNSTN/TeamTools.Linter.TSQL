SET XACT_ABORT ON

INSERT foo (a, b)
VALUES (1, 2)

UPDATE bar SET
    lasdmod = GETDATE()
WHERE id = @id

GO
BEGIN TRY
    -- still 1st delete can occur thus it is a multi DML
    IF 1=1
        DELETE t
        FROM tar

    MERGE trg
    USING src
    ON src.id = trg.id
    WHEN MATCHED THEN
        DELETE;
END TRY
BEGIN CATCH
    RETURN;
END CATCH
GO
