-- compatibility level min: 130
CREATE TABLE dbo.actives
(
    asset_code              VARCHAR(512)   NOT NULL
    , isin                  VARCHAR(20)    NULL
    , CONSTRAINT PK_actives PRIMARY KEY NONCLUSTERED HASH (asset_code) WITH (BUCKET_COUNT = 500000)
    , INDEX IX_dbo_actives_isin NONCLUSTERED HASH (isin)
          WITH (BUCKET_COUNT = 500000)
)
WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);
GO
