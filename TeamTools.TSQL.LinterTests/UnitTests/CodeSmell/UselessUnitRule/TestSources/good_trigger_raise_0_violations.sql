CREATE TRIGGER dbo.tr ON dbo.bar AFTER INSERT AS
BEGIN
    UPDATE t SET
        some_value = i.new_value
    FROM dbo.far AS t
    INNER JOIN INSERTED AS i
        ON i.id = t.id
    WHERE t.some_value IS NULL

    EXEC dbo.notify;

    RETURN;
END
GO
