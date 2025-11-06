CREATE TABLE dbo.tbl (
    bad_id FLOAT PRIMARY KEY -- 1
)
GO

CREATE CLUSTERED INDEX IX_TEST
ON dbo.tbl (bad_id) -- 2
GO
