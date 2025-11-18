CREATE TABLE dbo.tbl
(
    id          INT NOT NULL
    , parent_id INT NULL
    , name      VARCHAR(10) NULL
    , CONSTRAINT PK_tbl PRIMARY KEY (id)
)
GO
CREATE INDEX idx_parent_id on dbo.tbl(parent_id)
INCLUDE(name)
GO

CREATE TABLE dbo.foo
(
    some_id          INT          NOT NULL
    , CONSTRAINT PK_dbo_foo PRIMARY KEY CLUSTERED (some_id ASC)
)
GO
