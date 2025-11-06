CREATE TABLE dbo.UPPER_SNAKE_CASE
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

CREATE VIEW dbo.UPPER_SNAKE_CASE_VIEW
AS
SELECT * FROM dbo.foo
GO
