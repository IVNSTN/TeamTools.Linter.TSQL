CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
) ON partition_fn(sect_num)
GO
CREATE INDEX UQ_parent_id ON dbo.foo(name); -- not unique
