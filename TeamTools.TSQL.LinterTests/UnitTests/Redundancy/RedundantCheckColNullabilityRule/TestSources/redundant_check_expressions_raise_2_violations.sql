CREATE TABLE dbo.foo
(
    id          INT NOT NULL CHECK (ISNULL(id, 0) <> (0)),
    parent_id   INT NOT NULL CHECK  (NOT (parent_id IS NULL)),
    title       VARCHAR(100),
    CONSTRAINT ck_title CHECK(ISNULL(title, '') IS NOT NULL)
)
GO
