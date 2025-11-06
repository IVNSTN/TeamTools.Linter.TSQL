CREATE TABLE dbo.foo
(
    id          INT NOT NULL,
    parent_id   INT NULL CHECK (parent_id <> id),
    title       VARCHAR(100),
    CONSTRAINT ck_title CHECK(title <> '' OR parent_id = -1)
)
