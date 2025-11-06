-- no sparse
CREATE TABLE #foo (
    id INT NOT NULL,
    calc AS id + 1 PERSISTED
);

DECLARE @bar TABLE (
    id INT NOT NULL,
    calc AS id + 1 PERSISTED
)
GO

-- regular table
CREATE TABLE dbo.far (
    id INT NOT NULL,
    calc INT SPARSE NULL
);
GO
