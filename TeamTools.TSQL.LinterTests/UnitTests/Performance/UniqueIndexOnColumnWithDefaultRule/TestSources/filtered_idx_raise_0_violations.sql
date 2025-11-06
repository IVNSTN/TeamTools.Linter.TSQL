CREATE TABLE dbo.bar
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL DEFAULT 0
)
GO
CREATE UNIQUE INDEX UQ_parent_id ON dbo.bar(parent_id)
WHERE parent_id <> 0; -- filtered
