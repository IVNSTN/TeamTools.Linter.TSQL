MERGE t
USING
(
    SELECT NULL as value
) s
ON id = id
WHEN NOT MATCHED THEN
INSERT (value)
VALUES (s.value);
