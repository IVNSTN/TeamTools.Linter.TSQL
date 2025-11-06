CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL
)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(parent_id)
