CREATE TRIGGER tr ON foo
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #tmp
    (
        another_id INT
        , CONSTRAINT pk         -- named constraint
        PRIMARY KEY (another_id)
    )
END
GO
