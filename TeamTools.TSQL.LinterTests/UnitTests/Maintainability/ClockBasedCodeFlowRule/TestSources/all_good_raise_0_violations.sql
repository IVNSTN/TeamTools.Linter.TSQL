SELECT GETDATE(), SYSDATETIME(), @today

UPDATE t SET
    dtupdate = SYSDATETIME(),
    close_date = GETDATE()
FROM tbl as t
WHERE close_date IS NULL

WHILE @last_date < @today
    DELETE dbo.clients
