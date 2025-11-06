CREATE TRIGGER iud_trg
ON dbo.foo AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    INSERT dbo.bar
    SELECT * FROM INSERTED

    DELETE fr
        OUTPUT
        DELETED.ID
        INTO dbo.Log(ID)
    FROM dbo.far as far
    INNER JOIN DELETED as del
        ON del.ID = far.ID

    RAISERROR ('asdf', 16, 1);

    DECLARE @sum INT, @del_cnt INT
    
    SELECT @sum = SUM(i.value)
    FROM INSERTED AS i

    SET @del_cnt = (SELECT COUNT(*) FROM DELETED)

    SELECT 'test'
        into dbo.target
    FROM src

    -- PRINT 'test'
END
