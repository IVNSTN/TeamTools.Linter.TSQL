CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
) ON partition_fn(sect_num)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(name, sect_num) -- partitioned col included
ON partition_fn(sect_num);
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.foo(name, parent_id) -- partitioned on different col
ON other_partition_fn(parent_id);
