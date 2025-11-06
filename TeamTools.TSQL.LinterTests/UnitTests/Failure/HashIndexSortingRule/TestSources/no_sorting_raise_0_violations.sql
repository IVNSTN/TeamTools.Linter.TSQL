CREATE TABLE dbo.foo
(
    major_id        BIGINT          NOT NULL
    , secondary_id  INT             NOT NULL
    , CONSTRAINT pk_foo PRIMARY KEY NONCLUSTERED HASH (major_id) WITH (BUCKET_COUNT = 100000000)
    , INDEX ix_secondary_id NONCLUSTERED HASH (secondary_id) WITH (BUCKET_COUNT = 5000000)
)
WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);
GO
