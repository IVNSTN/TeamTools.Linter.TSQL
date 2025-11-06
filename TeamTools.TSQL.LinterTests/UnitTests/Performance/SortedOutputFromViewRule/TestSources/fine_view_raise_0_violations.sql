CREATE VIEW dbo.my_view
AS
SELECT id, title
FROM dbo.foo
GROUP BY id, title
