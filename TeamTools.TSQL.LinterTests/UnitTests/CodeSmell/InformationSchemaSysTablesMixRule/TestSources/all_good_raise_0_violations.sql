-- information_schema only
SELECT TOP 10 t.TABLE_SCHEMA, t.TABLE_NAME, c.COLUMN_NAME, c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
INNER JOIN INFORMATION_SCHEMA.TABLES t
ON t.TABLE_SCHEMA = c.TABLE_SCHEMA
AND t.TABLE_NAME = c.TABLE_NAME

-- sys views only
SELECT TOP 10
       s.name AS TABLE_SCHEMA
       , t.name AS TABLE_NAME
       , c.name AS COLUMN_NAME
       , c.is_nullable AS IS_NULLABLE
FROM sys.columns AS c
INNER JOIN sys.tables AS t
    ON c.object_id = t.object_id
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id;
