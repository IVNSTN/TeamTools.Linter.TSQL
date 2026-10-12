CREATE PROC my_proc
AS
BEGIN
    SET NOCOUNT ON;             -- reset

    SELECT TOP (10) * FROM tbl  -- set

    DECLARE @a INT              -- reset

    SET @a = @@ROWCOUNT         -- 1

    UPDATE t                    -- set
    SET upd = GETDATE()

    PRINT '='                   -- reset

    SELECT @@ROWCOUNT           -- 2

    RECEIVE TOP (1)             -- set
        @a = x
    FROM my_queue

    BEGIN TRAN                  -- reset

    IF @@ROWCOUNT > 0
        INSERT t (a) VALUES (@a)
END
GO
