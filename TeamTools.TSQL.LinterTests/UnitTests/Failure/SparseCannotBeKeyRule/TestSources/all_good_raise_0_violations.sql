CREATE TABLE dbo.bar
(
    group_id INT NOT NULL
    , col    BIT SPARSE  NULL
)
GO

CREATE NONCLUSTERED INDEX ix ON dbo.bar(col) -- nonclustered is fine
GO
