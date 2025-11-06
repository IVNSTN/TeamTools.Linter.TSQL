CREATE TABLE dbo.foo
(
    id          INT NOT NULL CHECK ((id) IS NOT NULL), -- redundant
    parent_id   INT CHECK (parent_id IS NULL), -- redundant, pointless
    title       VARCHAR(100),
    CONSTRAINT ck_title CHECK(title IS NOT NULL) -- move to column property
)
GO
ALTER TABLE dbo.foo ADD CONSTRAINT ck_parent_id
CHECK ((parent_id) IS NOT NULL); -- move to column property
