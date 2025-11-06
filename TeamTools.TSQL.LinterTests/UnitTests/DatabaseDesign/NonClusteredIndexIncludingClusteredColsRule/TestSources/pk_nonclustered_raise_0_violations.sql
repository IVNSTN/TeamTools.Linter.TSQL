CREATE TABLE dbo.foo
(
    id          INT NOT NULL
    , parent_id INT NULL
    , name      VARCHAR(10) NOT NULL
    , sect_num  TINYINT NOT NULL
    , CONSTRAINT PK_foo PRIMARY KEY NONCLUSTERED (id)
)
GO
CREATE CLUSTERED INDEX IX_parent_id on dbo.foo(parent_id);
GO
CREATE NONCLUSTERED INDEX UQ_name ON dbo.foo(name)
include(id); -- id is not clustered
GO
