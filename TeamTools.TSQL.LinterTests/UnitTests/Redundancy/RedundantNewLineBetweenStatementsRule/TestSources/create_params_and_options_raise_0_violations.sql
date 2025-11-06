CREATE PROCEDURE CashObjects.Notify
    @EventID       UNIQUEIDENTIFIER = NULL OUTPUT
    , @MessageType VARCHAR(128)
    , @Tag         VARBINARY(128)   = NULL
    , @Data        VARBINARY(MAX)
    , @ErrMess     VARCHAR(MAX)     = NULL OUTPUT
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;
END;
GO
CREATE TRIGGER dbo.my_trigger on dbo.my_table
WITH EXECUTE AS OWNER
AFTER INSERT
AS
BEGIN
    ROLLBACK;
END
GO
CREATE FUNCTION dbo.my_fn ()
RETURNS @tbl TABLE (
    id int
)
AS
BEGIN
    INSERT @tbl
    select id
    FROM src
END
