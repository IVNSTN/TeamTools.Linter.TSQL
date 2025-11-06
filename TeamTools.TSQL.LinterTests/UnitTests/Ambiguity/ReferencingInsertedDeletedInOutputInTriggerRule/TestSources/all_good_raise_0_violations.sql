CREATE TRIGGER iud_trg
ON dbo.foo AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DELETE fr
        OUTPUT
        DELETED.ID
        INTO dbo.Log(ID)
    FROM dbo.far as far
    INNER JOIN @DELETED as del
        ON del.ID = far.ID

    UPDATE t SET
        name = i.name
        OUTPUT
        INSERTED.name
    FROM target as t
    INNER JOIN #INSERTED AS i
        on i.id = t.id
END
