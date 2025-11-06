CREATE TABLE dbo.far
(
    id          INT NOT NULL PRIMARY KEY
    , parent_id INT NULL DEFAULT 0
)
GO
-- not unique
CREATE INDEX UQ_parent_id ON dbo.far(parent_id);
