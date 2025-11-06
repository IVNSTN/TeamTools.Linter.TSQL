CREATE TRIGGER trg on dbo.orders AFTER UPDATE
AS
BEGIN
    SELECT * FROM DELETED d inner join INSERTED i
    on i.id = d.id;
    
    exec ('cmd');
END
