CREATE TABLE dbo.foo
(
    id          INT NOT NULL PRIMARY KEY NONCLUSTERED
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
) -- heap
GO
CREATE NONCLUSTERED INDEX UQ_parent_id ON dbo.foo(parent_id, id);
