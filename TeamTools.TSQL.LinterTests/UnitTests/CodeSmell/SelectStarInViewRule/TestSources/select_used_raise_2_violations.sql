CREATE VIEW dbo.foo
AS
SELECT
    a, b, 'x' as [*]
FROM (select * from c) d
GO

CREATE VIEW dbo.bar
AS
SELECT
    *, d.a
FROM d
GO

CREATE VIEW dbo.zar
AS
SELECT
    a, d.*
FROM d
