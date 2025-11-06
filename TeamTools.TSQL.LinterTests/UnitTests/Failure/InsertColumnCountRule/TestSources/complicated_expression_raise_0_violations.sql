INSERT INTO @NewNotifications (IdNotification, IsSystem, IsActive)
SELECT
    new_notifications.IdNotification
    , new_notifications.IsSystem
    , new_notifications.IsActive -- fine
FROM
(
    MERGE mt.Notifications AS n
    USING @NotificationsTable AS t
    ON n.IdNotification = t.IdNotification
    WHEN NOT MATCHED THEN
        INSERT (Name, IsGroup, IsSystem, IsActive, Description)
        VALUES
        (t.Name, t.IsGroup, t.IsSystem, t.IsActive, t.Description) -- fine
    WHEN MATCHED THEN
        UPDATE SET
            DtUpdate = GETDATE()
            , Name = ISNULL(t.Name, n.Name)
            , IsGroup = ISNULL(t.IsGroup, n.IsGroup)
            , IsSystem = ISNULL(t.IsSystem, n.IsSystem)
            , IsActive = ISNULL(t.IsActive, n.IsActive)
            , Description = ISNULL(t.Description, n.Description)
    OUTPUT
        $action AS action
        , inserted.IdNotification
        , inserted.IsSystem
        , inserted.IsActive
)   AS new_notifications(action, IdNotification, IsSystem, IsActive) -- OUTPUT col count = defined col count
WHERE new_notifications.action = 'INSERT';
