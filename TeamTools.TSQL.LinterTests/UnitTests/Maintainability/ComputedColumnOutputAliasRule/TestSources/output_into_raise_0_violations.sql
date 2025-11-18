DELETE t
    OUTPUT 'DEL', DELETED.record_name
    INTO dbo.change_log(action_name, record_name)
FROM dbo.tbl as t
WHERE id = 1

MERGE t
USING s
on s.id = t.id
WHEN MATCHED THEN
    UPDATE SET dt = GETDATE()
    OUTPUT INSERTED.dt + 1
    INTO dbo.change_log(dt);
