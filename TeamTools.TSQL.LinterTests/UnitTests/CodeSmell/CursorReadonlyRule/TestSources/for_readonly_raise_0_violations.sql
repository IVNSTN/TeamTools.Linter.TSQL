DECLARE ListDB CURSOR LOCAL STATIC FOR
    SELECT db.name
    FROM master.sys.dm_hadr_availability_replica_states AS hars
    INNER JOIN master.sys.availability_groups AS ag
        ON hars.group_id = ag.group_id
    INNER JOIN master.sys.dm_hadr_database_replica_cluster_states AS dbcs
        ON dbcs.replica_id = hars.replica_id
    RIGHT JOIN master.sys.databases AS db
        ON db.name = dbcs.database_name
    WHERE ISNULL(hars.role_desc, 'primary') <> 'secondary'
        AND db.is_broker_enabled = 1
        AND db.state = 0
        AND db.database_id >= 5
        AND db.source_database_id IS NULL
    FOR READ ONLY;
