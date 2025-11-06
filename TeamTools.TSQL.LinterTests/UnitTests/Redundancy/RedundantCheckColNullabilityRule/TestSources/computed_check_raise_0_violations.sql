CREATE TABLE dbo.foo
(
    id          INT NOT NULL
    , parent_id INT NULL CHECK (parent_id <> id)
    , title     VARCHAR(100)
    , CONSTRAINT chk CHECK (COALESCE(parent_id, id) IS NOT NULL)
);
