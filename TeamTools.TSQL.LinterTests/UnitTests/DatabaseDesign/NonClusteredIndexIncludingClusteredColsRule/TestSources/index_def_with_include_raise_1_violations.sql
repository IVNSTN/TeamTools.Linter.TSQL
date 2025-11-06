-- compatibility level min: 150
CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY -- by default it is clustered
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
    , INDEX UQ_name (name) INCLUDE (id) -- here
)
