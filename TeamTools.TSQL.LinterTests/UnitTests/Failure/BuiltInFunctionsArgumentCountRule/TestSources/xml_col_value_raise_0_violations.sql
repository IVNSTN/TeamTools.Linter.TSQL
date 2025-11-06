SELECT
    event_data.value('(/event/@name)[1]', 'VARCHAR(100)') AS event_name
FROM #Event
