CREATE TRIGGER iud_trg
ON dbo.foo AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DELETE fr
        OUTPUT
        DELETED.ID
        INTO dbo.Log(ID)
    FROM dbo.far as far
    INNER JOIN DELETED
        ON DELETED.ID = far.ID

    UPDATE t SET
        name = i.name
        OUTPUT
        INSERTED.name
    FROM target as t
    INNER JOIN INSERTED
        on INSERTED.id = t.id
END
