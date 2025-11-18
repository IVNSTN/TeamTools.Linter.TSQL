CREATE PROC dbo.foo
AS
BEGIN
    DECLARE
        @ProcName  VARCHAR(256)  = OBJECT_SCHEMA_NAME(@@PROCID) + '.' + OBJECT_NAME(@@PROCID) -- this would give 'dbo.foo' which has no unicode for sure
END
GO
