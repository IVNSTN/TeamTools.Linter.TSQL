CREATE TABLE dbo.foo
(
    id INT,
    title VARCHAR(100),
    foo_type_id INT
)
GO

ALTER TABLE dbo.foo
ADD lastmod DATETIME NULL -- 1

GO
ALTER TABLE dbo.foo
DROP COLUMN title -- 2
GO
