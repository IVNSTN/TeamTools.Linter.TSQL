CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(name, parent_id)
WHERE name is not null
