-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NOT NULL -- not null no default
    , UNIQUE (parent_id)
    , INDEX ix UNIQUE (parent_id)
)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(parent_id)
