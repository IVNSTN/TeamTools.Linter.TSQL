-- compatibility level min: 130
CREATE PROC my_proc
WITH NATIVE_COMPILATION, SCHEMABINDING, EXECUTE AS OWNER
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'English')
    -- the rule must not think about @@ROWCOUNT usage, it only should not report this as violation
    SET @a = @@ROWCOUNT
END
GO
