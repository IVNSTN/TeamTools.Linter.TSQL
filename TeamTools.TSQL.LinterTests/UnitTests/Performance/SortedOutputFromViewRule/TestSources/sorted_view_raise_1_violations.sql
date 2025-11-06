CREATE VIEW dbo.my_view
AS
    SELECT TOP (100) PERCENT
           id
           , title
    FROM dbo.foo
    ORDER BY id;
