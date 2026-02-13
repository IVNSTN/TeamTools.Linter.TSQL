
IF @dt > GETDATE()
BEGIN
    PRINT 'FUTURE'
    RETURN;
END
ELSE
BEGIN
    BEGIN
        PRINT 'FUTURE'

        -- comment
        RETURN
    END
END
