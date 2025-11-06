            UPDATE t SET
    dtupdate = SYSDATETIME(),
    close_date = GETDATE()
FROM tbl as t
WHERE close_date < GETDATE()
