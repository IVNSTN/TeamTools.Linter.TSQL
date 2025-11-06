-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY -- by default it is clustered
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
    , INDEX UQ_parent_id (parent_id, id)
) WITH (MEMORY_OPTIMIZED=ON)
