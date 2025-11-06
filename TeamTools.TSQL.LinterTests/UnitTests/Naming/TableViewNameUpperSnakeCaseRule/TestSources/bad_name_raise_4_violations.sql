CREATE TABLE dbo.lower_snake_case
(
    id INT not null
)
GO
CREATE TABLE #camelCase
(
    id INT not null
)
GO
DECLARE @camelCase TABLE
(
    id INT not null
)
GO

CREATE VIEW dbo.lower_snake_case_view
AS
SELECT * FROM dbo.foo
GO
