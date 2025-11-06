CREATE TRIGGER ddl_trigger_raise_0_violations
ON DATABASE
FOR DDL_DATABASE_LEVEL_EVENTS
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @event XML = EVENTDATA();

    INSERT audit.ddl (datetime, login, type, name, stmt)
    VALUES
    (
        @event.value('(/EVENT_INSTANCE/PostTime)[1]', 'datetime')
        , @event.value('(/EVENT_INSTANCE/LoginName)[1]', 'nvarchar(128)')
        , @event.value('(/EVENT_INSTANCE/EventType)[1]', 'nvarchar(128)')
        , ISNULL(NULLIF(@event.value('(/EVENT_INSTANCE/SchemaName)[1]', 'nvarchar(128)'), '') + '.', '')
          + @event.value('(/EVENT_INSTANCE/ObjectName)[1]', 'nvarchar(128)')
        , @event.value('(/EVENT_INSTANCE/TSQLCommand/CommandText)[1]', 'nvarchar(max)')
    );
END;
