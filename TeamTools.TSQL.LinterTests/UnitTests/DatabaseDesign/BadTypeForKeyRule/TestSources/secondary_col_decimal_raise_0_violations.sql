CREATE TABLE dbo.tbl
(
    id          VARCHAR(128)
    , type_id   SMALLINT
    , num       BIGINT
    , dt        DATE
    , CONSTRAINT PK PRIMARY KEY CLUSTERED (type_id, num)
)
