CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL        -- not partitioned
)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(name);
