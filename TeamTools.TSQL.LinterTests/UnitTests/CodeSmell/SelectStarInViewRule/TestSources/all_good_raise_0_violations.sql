CREATE VIEW dbo.foo
AS
SELECT
    a, b, 'x' as [*]
FROM (select * from c) d
GO
CREATE VIEW dbo.bar
AS
SELECT 1
UNION 
SELECT 2
