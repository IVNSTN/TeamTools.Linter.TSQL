CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NOT NULL -- not null no default
)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(parent_id)
