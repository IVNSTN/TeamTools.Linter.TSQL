CREATE PROC foo
    @client_id INT
WITH EXECUTE AS OWNER,
RECOMPILE                       -- 1 - RECOMPILE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #tmp
    (
        id INT
    )

    DROP TABLE #tmp

    CREATE TABLE #tmp           -- 2 - name reused
    (
        another_id INT
        , CONSTRAINT pk         -- 3 - named constraint
        PRIMARY KEY (another_id)
    )

    ALTER TABLE #tmp            -- 4 - DDL ALTER
    ADD title VARCHAR(100)

    CREATE INDEX ix             -- 5 - DDL INDEX
    ON #tmp (another_id)

    CREATE STATISTICS st        -- 6 - DDL STATISTICS
    ON #tmp (title)

    RETURN 1
END
GO
