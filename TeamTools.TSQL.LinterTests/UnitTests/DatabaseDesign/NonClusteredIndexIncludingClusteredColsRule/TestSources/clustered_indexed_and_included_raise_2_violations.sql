CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY -- by default it is clustered
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
) ON partition_fn(sect_num)
GO
CREATE NONCLUSTERED INDEX UQ_parent_id ON dbo.foo(parent_id, id); -- clustered field indexed
GO
CREATE INDEX UQ_name ON dbo.foo(name)
INCLUDE (id)                                                      -- clustered field included
