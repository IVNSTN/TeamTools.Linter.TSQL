-- compatibility level min: 150
CREATE TABLE push_ntf.corp_actions
(
    income_type_id     SMALLINT    NOT NULL
    , income_type_name VARCHAR(50) NOT NULL
    , CONSTRAINT PK_push_ntf_corp_actions PRIMARY KEY NONCLUSTERED HASH (income_type_id) WITH (BUCKET_COUNT = 50)
)
WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);
GO
