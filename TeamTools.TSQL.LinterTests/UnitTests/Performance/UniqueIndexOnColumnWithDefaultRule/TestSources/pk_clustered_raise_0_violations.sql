CREATE TABLE dbo.foo
(
    id          INT NOT NULL
    , parent_id INT NOT NULL DEFAULT 0
    , CONSTRAINT PK PRIMARY KEY CLUSTERED (parent_id)
)
GO
