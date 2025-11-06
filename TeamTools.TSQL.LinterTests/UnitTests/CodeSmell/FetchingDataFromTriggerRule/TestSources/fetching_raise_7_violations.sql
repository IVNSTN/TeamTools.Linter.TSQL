CREATE TRIGGER iud_trg
ON dbo.foo AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SELECT * FROM INSERTED      -- 1

    DELETE fr
        OUTPUT                  -- 2
        DELETED.ID
    FROM dbo.far as far
    INNER JOIN DELETED as del
        ON del.ID = far.ID

    RAISERROR ('asdf', 10, 1);  -- 3

    DECLARE @severity int
    RAISERROR ('asdf', @severity, 1);  -- ignored

    PRINT 'test'                -- 4

    SELECT SUM(i.value)         -- 5
    FROM INSERTED AS i

    SELECT                      -- 6
        (SELECT COUNT(*) FROM DELETED)

    SELECT 'test'
        -- into dbo.target      -- 7
    FROM src
END
