-- not inmemory
CREATE TABLE dbo.foo
(
    major_id        BIGINT          NOT NULL
    , secondary_id  INT             NOT NULL
    , CONSTRAINT pk_foo PRIMARY KEY CLUSTERED (major_id ASC)
    , INDEX ix_secondary_id NONCLUSTERED (major_id, secondary_id DESC)
);
GO

CREATE TABLE dbo.foo
(
    major_id        BIGINT          NOT NULL
    , secondary_id  INT             NOT NULL
    -- hash index not sorted
    , CONSTRAINT pk_foo PRIMARY KEY NONCLUSTERED HASH (major_id) WITH (BUCKET_COUNT = 1)
    -- sorted
    , INDEX ix_secondary_id NONCLUSTERED (secondary_id ASC) WITH (BUCKET_COUNT = 5)
    , INDEX ix_secondary_id2 NONCLUSTERED (secondary_id DESC, major_id ASC) WITH (BUCKET_COUNT = 5)
)
WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);
GO
