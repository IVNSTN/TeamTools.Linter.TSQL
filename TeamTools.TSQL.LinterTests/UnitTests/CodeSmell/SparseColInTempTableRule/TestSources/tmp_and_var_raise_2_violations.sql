CREATE TABLE #foo (
    id INT NOT NULL,
    calc INT SPARSE NULL -- 1
);

DECLARE @bar TABLE (
    id INT NOT NULL,
    calc INT SPARSE NULL -- 2
)
GO
