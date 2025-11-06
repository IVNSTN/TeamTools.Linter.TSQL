CREATE TABLE dbo.foo
(
    id INT
    , parent_id INT
    , CONSTRAINT FK_parent FOREIGN KEY (parent_id) REFERENCES foo(id)
)
GO
ALTER TABLE dbo.foo
ADD CONSTRAINT FK_parent FOREIGN KEY (parent_id) REFERENCES dbo.foo(id)
