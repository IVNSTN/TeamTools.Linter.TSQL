CREATE TABLE dbo.PascalCase
(
    id INT not null
)
GO
CREATE TABLE #PascalCase
(
    id INT not null
)
GO
DECLARE @PascalCase TABLE
(
    id INT not null
)
GO

CREATE VIEW dbo.PascalCaseView
AS
SELECT * FROM dbo.foo
GO
