SELECT
    title,
    NULL as parent_id,
    (
        SELECT src.group_id, NULL as is_deleted
        FOR XML AUTO, TYPE
    ) as xml_value
FROM src
FOR XML PATH('node')
