-- compatibility level min: 130
INSERT @batch (group_id, group_code)
SELECT
    group_id
    , group_code
FROM
    OPENJSON(@data)
        WITH (group_id INT, group_code VARCHAR(100));
