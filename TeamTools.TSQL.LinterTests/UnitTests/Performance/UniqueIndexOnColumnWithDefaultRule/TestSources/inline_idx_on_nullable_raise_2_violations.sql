-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL
    , UNIQUE (parent_id) -- 1
    , INDEX ix UNIQUE (parent_id) -- 2
)
