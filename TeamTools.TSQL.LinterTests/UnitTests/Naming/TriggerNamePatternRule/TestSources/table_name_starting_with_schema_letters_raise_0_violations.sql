CREATE TRIGGER fi.fi_rollback_when_no_access -- fi is table name, not schema prefix
ON fi.fi
AFTER DELETE
AS
    RETURN;
GO
CREATE TRIGGER zoo.zoom_rollback_when_no_access
ON zoo.zoom
AFTER DELETE
AS
    RETURN;
GO
