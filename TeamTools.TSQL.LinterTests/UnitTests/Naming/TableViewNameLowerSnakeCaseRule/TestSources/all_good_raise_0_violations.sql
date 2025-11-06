CREATE TABLE dbo.lower_snake_case
(
    id INT not null
)
GO
CREATE TABLE #lower_snake_case
(
    id INT not null
)
GO
DECLARE @lower_snake_case TABLE
(
    id INT not null
)
GO

CREATE VIEW dbo.lower_snake_case_view
AS
SELECT * FROM dbo.foo
GO
