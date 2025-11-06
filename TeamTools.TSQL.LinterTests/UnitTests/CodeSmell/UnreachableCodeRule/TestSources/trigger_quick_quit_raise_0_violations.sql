            BEGIN
IF @@ROWCOUNT = 0
BEGIN
    RETURN;
END;

SET NOCOUNT ON;
            END
