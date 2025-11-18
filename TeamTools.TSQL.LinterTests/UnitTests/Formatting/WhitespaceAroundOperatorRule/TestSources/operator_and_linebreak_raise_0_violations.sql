-- no space after = but line break is there
SET @packet =
(
    SELECT
        'A' AS type
        , 0 AS shard_key
)

BEGIN
    -- linebreak and many spaces
    SET @packet =
    (
        SELECT
            'A' AS type
            , 0 AS shard_key
    )
END
