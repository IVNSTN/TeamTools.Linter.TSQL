SELECT TOP 10
       s.name AS TABLE_SCHEMA
       , t.name AS TABLE_NAME
       , c.COLUMN_NAME
       , c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS AS c
INNER JOIN sys.tables AS t
    ON t.name = c.TABLE_NAME
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
    AND s.name = c.TABLE_SCHEMA
