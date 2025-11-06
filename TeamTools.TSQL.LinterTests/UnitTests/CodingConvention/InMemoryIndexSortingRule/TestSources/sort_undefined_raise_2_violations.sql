CREATE TABLE dbo.foo
(
    major_id        BIGINT          NOT NULL
    , secondary_id  INT             NOT NULL
    , CONSTRAINT pk_foo PRIMARY KEY NONCLUSTERED (major_id) WITH (BUCKET_COUNT = 100000000)
    , INDEX ix_secondary_id NONCLUSTERED (secondary_id) WITH (BUCKET_COUNT = 5000000) -- 1
    , INDEX ix_secondary_id2 NONCLUSTERED (secondary_id DESC, major_id) WITH (BUCKET_COUNT = 5000000) -- 2
)
WITH (MEMORY_OPTIMIZED = ON);
GO
