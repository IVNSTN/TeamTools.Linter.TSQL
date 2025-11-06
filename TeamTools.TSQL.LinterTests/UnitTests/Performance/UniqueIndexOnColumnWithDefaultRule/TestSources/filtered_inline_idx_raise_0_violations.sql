-- compatibility level min: 130
CREATE TABLE dbo.bar
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL DEFAULT 0
    , INDEX ix UNIQUE (parent_id) WHERE parent_id <> 0
)
GO
